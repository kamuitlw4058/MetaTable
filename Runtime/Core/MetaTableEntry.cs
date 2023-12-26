using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using Sirenix.OdinInspector;
using ClassGenerator;
using UnityEngine;
using Pangoo.Common;



#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
#endif

namespace MetaTable
{

    [Serializable]
    public class MetaTableEntry
    {
        public enum MetaTableTypeEnum
        {
            Define,
            Reference
        }


        [ReadOnly]
        [VerticalGroup("基本信息")]
        [LabelText("配置")]
        public MetaTableConfig Config;


        [VerticalGroup("基本信息")]
        [LabelText("表类型")]
        public MetaTableTypeEnum MetaTableType;


        // [ValueDropdown("GetNamespaces")]
        [VerticalGroup("基本信息")]
        [LabelText("命名空间")]
        public string Namespace
        {
            get
            {
                return Config.Namespace;
            }
        }

        // [ReadOnly]
        [VerticalGroup("基本信息")]
        [LabelText("表名")]
        [TableColumnWidth(240, resizable: false)]
        [ShowIf("MetaTableType", MetaTableTypeEnum.Define)]
        public string TableName;

        // [VerticalGroup("基本信息")]
        // [LabelText("生成接口")]
        // // [TableColumnWidth(240, resizable: false)]
        // [ShowIf("MetaTableType", MetaTableTypeEnum.Define)]
        // public bool IsGenerateInterface;

        [VerticalGroup("内容编辑")]
        [LabelText("引用配置")]
        [ShowIf("MetaTableType", MetaTableTypeEnum.Reference)]
        [ValueDropdown("OnRefConfigDropdown")]
        public MetaTableConfig RefConfig;

#if UNITY_EDITOR
        IEnumerable OnRefConfigDropdown()
        {
            ValueDropdownList<MetaTableConfig> list = new ValueDropdownList<MetaTableConfig>();
            var assets = AssetDatabaseUtility.FindAsset<MetaTableConfig>();
            foreach (var asset in assets)
            {
                list.Add($"{asset.Namespace}", asset);


            }
            return list;
        }
#endif

        [VerticalGroup("内容编辑")]
        [LabelText("引用表名")]
        [ShowIf("MetaTableType", MetaTableTypeEnum.Reference)]
        [ValueDropdown("OnRefTableNameDropdown")]
        public string RefTableName;

        IEnumerable OnRefTableNameDropdown()
        {
            ValueDropdownList<string> list = new();
            if (RefConfig != null && RefConfig.Entries.Count > 0)
            {
                foreach (var entry in RefConfig.Entries)
                {
                    list.Add(entry.BaseName);
                }
            }

            return list;
        }


        [VerticalGroup("内容编辑")]
        [LabelText("表头")]
        [ShowIf("MetaTableType", MetaTableTypeEnum.Define)]

        [TableList(AlwaysExpanded = true)]
        public List<MetaTableColumn> Columns = new List<MetaTableColumn>();

        public bool HasId
        {
            get
            {
                foreach (var column in Columns)
                {
                    if (column.Name.ToLower() == "id")
                    {
                        return true;
                    }
                }
                return false;
            }
        }

#if UNITY_EDITOR
        public void UpdateColumnsByExcel(bool replaceId2Uuid = false)
        {
            if (TableName.IsNullOrWhiteSpace() || Config == null)
            {
                return;
            }


            TableName = NameUtility.ToTitleCase(TableName);



            var excelFilePath = Path.Join(Config.StreamResExcelDir, $"{TableName}.xlsx").PathReplace();
            if (!File.Exists(excelFilePath))
            {
                return;
            }

            var excelColumns = ExcelHelper.ParserEPPlusColumns(excelFilePath);
            if (excelColumns != null)
            {
                Columns = excelColumns;
            }

            if (replaceId2Uuid)
            {
                foreach (var column in Columns)
                {
                    if (column.Name.Equals("Id"))
                    {
                        column.Name = "Uuid";
                        column.Type = "string";
                        column.CnName = "Uuid";
                    }
                }
            }
        }


        [Button("从Excel刷新列头")]
        [BoxGroup("基本信息/操作")]
        [ShowIf("MetaTableType", MetaTableTypeEnum.Define)]
        public void UpdateColumnsByExcel()
        {
            UpdateColumnsByExcel(false);
        }

#endif

        public string BaseName => NameUtility.ToTitleCase(TableName);

        public string OverviewName => $"{BaseName}Overview";
#if UNITY_EDITOR
        #region 生成脚本
        [Button("生成脚本")]
        [BoxGroup("基本信息/操作")]
        [ShowIf("MetaTableType", MetaTableTypeEnum.Define)]

        public void GeneratorCode()
        {
            if (Columns.Count == 0 || Config == null)
            {
                return;
            }



            var classBaseName = NameUtility.ToTitleCase(TableName);
            var rowName = $"{classBaseName}Row";


            var classGenerateDir = Path.Join(Config.ScriptGenerateDir, classBaseName);
            DirectoryUtility.ExistsOrCreate(classGenerateDir);

            var tableName = NameUtility.ToTitleCase($"{classBaseName}Table");

            var rowJson = CodeGeneratorJson.BuildRowCodeJson(Columns);
            if (rowJson == null)
            {
                return;
            }

            var interfaceRowName = $"I{classBaseName}Row";

            GenerateRowInterface(classGenerateDir, interfaceRowName, rowJson, classBaseName);

            GenerateRow(classGenerateDir, rowName, rowJson, classBaseName, interfaceRowName);

            var interfaceTableName = $"I{classBaseName}Table";
            GenerateTableInterface(classGenerateDir, interfaceTableName, interfaceRowName, classBaseName);

            var codeTableName = $"{classBaseName}Table";
            GeneratorTable(codeTableName, classGenerateDir, interfaceRowName, classBaseName, interfaceTableName, interfaceRowName);


            var unityRowName = $"Unity{classBaseName}Row";
            GeneratorUnityRow(classBaseName, unityRowName, classGenerateDir, rowName);

            var codeOverviewName = $"{classBaseName}Overview";
            GeneratorOverview(classBaseName, codeOverviewName, unityRowName, rowName, codeTableName);

            var newRowWrapperName = $"{classBaseName}NewRowWrapper";
            GeneratorNewRowWrapper(classBaseName, codeOverviewName, unityRowName, newRowWrapperName);

            var detailRowWrapperName = $"{classBaseName}DetailRowWrapper";
            GeneratorDetailRowWrapper(classBaseName, codeOverviewName, unityRowName, detailRowWrapperName);

            // var newRowWrapperName = $"{classBaseName}RowWrapper";
            GeneratorRowWrapper(classBaseName, codeOverviewName, unityRowName, newRowWrapperName);

            GeneratorOverviewWrapper(classBaseName, codeOverviewName, unityRowName);


            AssetDatabase.Refresh();
        }
        public void GenerateRow(string classGenerateDir, string rowName, string rowJson, string baseName, string interfaceRowName)
        {
            //Generator
            var codeRowPath = Path.Join(classGenerateDir, $"{rowName}.cs");
            JsonClassGenerator.GeneratorCodeString(rowJson, Namespace, new CSharpCodeRowWriter(Config.UsingNamespace, Columns, interfaceRowName), rowName, codeRowPath, baseClass: "MetaTableRow", baseFields: new string[] { "Uuid", "Name", "Id" });

            // Custom Code
            var classCustomDir = Path.Join(Config.ScriptCustomDir, baseName);
            DirectoryUtility.ExistsOrCreate(classCustomDir);
            var codeRowCustomPath = Path.Join(classCustomDir, $"{rowName}.Custom.cs");
            if (!File.Exists(codeRowCustomPath))
            {
                JsonClassGenerator.GeneratorCodeString("{}", Namespace, new CSharpCodeMetaTableBaseWriter(Config.UsingNamespace), rowName, codeRowCustomPath, isSerializable: false, isWriteFileHeader: false);
            }
        }

        public void GenerateRowInterface(string classGenerateDir, string interfaceRowName, string rowJson, string baseName)
        {
            DirectoryUtility.ExistsOrCreate(classGenerateDir);
            var codeRowPath = Path.Join(classGenerateDir, $"{interfaceRowName}.cs");

            JsonClassGenerator.GeneratorCodeString(rowJson, Namespace, new CSharpCodeInterfaceWriter(Config.UsingNamespace), interfaceRowName, codeRowPath, baseClass: "IMetaTableRow", baseFields: new string[] { "Uuid", "Name", "Id" });


            var classCustomDir = Path.Join(Config.ScriptCustomDir, baseName);
            DirectoryUtility.ExistsOrCreate(classCustomDir);

            var codeInterfaceCustomPath = Path.Join(classCustomDir, $"{interfaceRowName}.Custom.cs");
            if (!File.Exists(codeInterfaceCustomPath))
            {
                JsonClassGenerator.GeneratorCodeString("{}", Namespace, new CSharpCodeInterfaceWriter(Config.UsingNamespace), interfaceRowName, codeInterfaceCustomPath, isSerializable: false, isWriteFileHeader: false);
            }
        }




        public void GeneratorTable(string codeTableName, string classGenerateDir, string classRowName, string classBaseName, string interfaceTableName, string interfaceRowName)
        {
            var codeTablePath = Path.Join(classGenerateDir, $"{codeTableName}.cs");
            JsonClassGenerator.GeneratorCodeString("{}", Namespace, new CSharpCodeMetaTableBaseWriter(Config.UsingNamespace, (config, sw) =>
            {
                sw.WriteLine();
                sw.WriteLine($"        public Dictionary<string, {interfaceRowName}> RowDict = new Dictionary<string, {interfaceRowName}>();");


                sw.WriteLine();
                sw.WriteLine($"        public override IReadOnlyList<IMetaTableRow> BaseRows => RowDict.Values.ToList();");


                sw.WriteLine();
                sw.WriteLine($"        public override void AddRows(IReadOnlyList<IMetaTableRow> rows)");
                sw.WriteLine("        {");
                sw.WriteLine($"             for (int i = 0; i < rows.Count; i++)");
                sw.WriteLine("            {");
                sw.WriteLine($"               var o = rows[i];");
                sw.WriteLine($"               if (o.Uuid == null)");
                sw.WriteLine("                {");
                sw.WriteLine($"                   Debug.LogError(\"AddRows Uuid Is Null\");");
                sw.WriteLine($"                   return;");
                sw.WriteLine("                }");

                sw.WriteLine($"               if (RowDict.ContainsKey(o.Uuid))");
                sw.WriteLine("                {");
                sw.WriteLine("                   Debug.LogError($\"{GetType().Name} Uuid:{o.Uuid} Dup! Please Check\");");
                sw.WriteLine($"                   return;");
                sw.WriteLine("                }");

                sw.WriteLine($"                   RowDict.Add(o.Uuid, o as {interfaceRowName});");
                sw.WriteLine("            }");
                sw.WriteLine("        }");


                sw.WriteLine();
                sw.WriteLine($"        public override void MergeRows(IReadOnlyList<IMetaTableRow> rows)");
                sw.WriteLine("        {");
                sw.WriteLine($"            for (int i = 0; i < rows.Count; i++)");
                sw.WriteLine("            {");
                sw.WriteLine($"                if (!RowDict.ContainsKey(rows[i].Uuid))");
                sw.WriteLine("                {");
                sw.WriteLine($"                    RowDict.Add(rows[i].Uuid, rows[i] as {interfaceRowName});");
                sw.WriteLine("                }");
                sw.WriteLine("            }");
                sw.WriteLine("        }");



                sw.WriteLine();
                sw.WriteLine($"        public {interfaceRowName} GetRowByUuid(string uuid)");
                sw.WriteLine("        {");
                sw.WriteLine($"            if (RowDict.TryGetValue(uuid, out {interfaceRowName} row))");
                sw.WriteLine("            {");
                sw.WriteLine($"                return row;");
                sw.WriteLine("            }");
                sw.WriteLine($"            return null;");
                sw.WriteLine("        }");



                sw.WriteLine();
                sw.WriteLine($"        public {interfaceRowName} GetRowById(int id)");
                sw.WriteLine("        {");
                sw.WriteLine($"            var Values = RowDict.Values;");
                sw.WriteLine($"            foreach (var val in Values)");
                sw.WriteLine("            {");
                sw.WriteLine($"                if (val.Id == id)");
                sw.WriteLine("                {");
                sw.WriteLine($"                    return val;");
                sw.WriteLine("                }");
                sw.WriteLine("            }");
                sw.WriteLine($"            return null;");
                sw.WriteLine("        }");


                sw.WriteLine();
                sw.WriteLine($"        public override IMetaTableRow GetMetaTableRowByUuid(string uuid)");
                sw.WriteLine("        {");
                sw.WriteLine($"            return GetRowByUuid(uuid);");
                sw.WriteLine("        }");



                sw.WriteLine();
                sw.WriteLine($"        public override IMetaTableRow GetMetaTableRowById(int id)");
                sw.WriteLine("        {");
                sw.WriteLine($"            return GetRowById(id);");
                sw.WriteLine("        }");


                sw.WriteLine();
                sw.WriteLine($"        public override string TableName => \"{classBaseName}\";");

            }), codeTableName, codeTablePath, baseClass: $"MetaTableBase,{interfaceTableName}");
        }

        public void GenerateTableInterface(string classGenerateDir, string interfaceTableName, string interfaceRowName, string baseName)
        {
            DirectoryUtility.ExistsOrCreate(classGenerateDir);
            var codeInterfaceTablePath = Path.Join(classGenerateDir, $"{interfaceTableName}.cs");

            JsonClassGenerator.GeneratorCodeString("{}", Namespace, new CSharpCodeInterfaceWriter(Config.UsingNamespace, (config, sw) =>
            {
                sw.WriteLine();
                sw.WriteLine($"        {interfaceRowName} GetRowByUuid(string uuid);");

                sw.WriteLine();
                sw.WriteLine($"        {interfaceRowName} GetRowById(int id);");

            }), interfaceTableName, codeInterfaceTablePath, baseClass: "IMetaTableBase");
        }



        public void GeneratorUnityRow(string classBaseName, string codeUnityRowName, string classGenerateDir, string classRowName)
        {
            var codeUnityRowPath = Path.Join(classGenerateDir, $"{codeUnityRowName}.cs");
            // Generator Code
            JsonClassGenerator.GeneratorCodeString("{}", Namespace, new CSharpCodeMetaTableBaseWriter(Config.UsingNamespace, (config, sw) =>
            {

                sw.WriteLine($"        [HideLabel]");
                sw.WriteLine($"        public {classRowName} Row = new();");

                sw.WriteLine();
                sw.WriteLine($"        public override MetaTableRow BaseRow => Row;");

                sw.WriteLine();
                sw.WriteLine("#if UNITY_EDITOR");

                sw.WriteLine();
                sw.WriteLine($"        public override void SetRow(MetaTableRow row)");
                sw.WriteLine("        {");
                sw.WriteLine($"           Row = row as {classRowName};");
                sw.WriteLine("        }");

                sw.WriteLine();
                sw.WriteLine($"        public override MetaTableRow CloneRow()");
                sw.WriteLine("        {");
                sw.WriteLine($"           return CopyUtility.Clone<{classRowName}>(Row);");
                sw.WriteLine("        }");

                sw.WriteLine("#endif");


            }), codeUnityRowName, codeUnityRowPath, baseClass: "MetaTableUnityRow", isAddCreateAssetMenu: true, assetMenuPrefix: "MetaTable");

            // Custom Code
            var classCustomDir = Path.Join(Config.ScriptCustomDir, classBaseName);
            DirectoryUtility.ExistsOrCreate(classCustomDir);
            var codeUnityRowCustomPath = Path.Join(classCustomDir, $"{codeUnityRowName}.Custom.cs");
            if (!File.Exists(codeUnityRowCustomPath))
            {
                JsonClassGenerator.GeneratorCodeString("{}", Namespace, new CSharpCodeMetaTableBaseWriter(Config.UsingNamespace), codeUnityRowName, codeUnityRowCustomPath, isSerializable: false, isWriteFileHeader: false);
            }
        }


        public void GeneratorOverview(string classBaseName, string codeOverviewName, string codeUnityRowName, string rowName, string codeTableName)
        {
            var classGenerateDir = Path.Join(Config.ScriptGenerateDir, classBaseName);
            var codeOverviewPath = Path.Join(classGenerateDir, $"{codeOverviewName}.cs");
            JsonClassGenerator.GeneratorCodeString("{}", Namespace, new CSharpCodeMetaTableBaseWriter(Config.UsingNamespace, (config, sw) =>
            {
                sw.WriteLine();

                sw.WriteLine($"        [TableList(AlwaysExpanded = true)]");
                sw.WriteLine($"        public List<{codeUnityRowName}> Rows = new();");




                sw.WriteLine();
                sw.WriteLine($"        public override string TableName => \"{classBaseName}\";");

                sw.WriteLine();
                sw.WriteLine($"         public override IReadOnlyList<MetaTableUnityRow> UnityBaseRows => Rows;");


                sw.WriteLine();
                sw.WriteLine($"        public override MetaTableBase ToTable()");
                sw.WriteLine("        {");
                sw.WriteLine($"           return ToTable<{codeTableName}>();");
                sw.WriteLine("        }");

                sw.WriteLine("#if UNITY_EDITOR");



                sw.WriteLine();
                sw.WriteLine($"        public static {codeUnityRowName} GetUnityRowById(int id, string packageDir = null)");
                sw.WriteLine("        {");
                sw.WriteLine($"           var overviews = AssetDatabaseUtility.FindAsset<{codeOverviewName}>(packageDir);");
                sw.WriteLine($"           foreach (var overview in overviews)");
                sw.WriteLine("            {");
                sw.WriteLine($"               foreach (var row in overview.Rows)");
                sw.WriteLine("                {");
                sw.WriteLine($"                   if (row.Row.Id == id)");
                sw.WriteLine("                    {");
                sw.WriteLine($"                       return row;");
                sw.WriteLine("                    }");
                sw.WriteLine("                }");
                sw.WriteLine("            }");
                sw.WriteLine("             return null; ");
                sw.WriteLine("        }");


                sw.WriteLine();
                sw.WriteLine($"        public static IEnumerable GetIdDropdown(List<int> excludeIds = null, string packageDir = null)");
                sw.WriteLine("        {");
                sw.WriteLine($"           var ret = new ValueDropdownList<int>();");
                sw.WriteLine($"           var overviews = AssetDatabaseUtility.FindAsset<{codeOverviewName}>(packageDir);");
                sw.WriteLine($"           foreach (var overview in overviews)");
                sw.WriteLine("            {");
                sw.WriteLine($"               foreach (var row in overview.Rows)");
                sw.WriteLine("                {");
                sw.WriteLine($"                   bool flag = excludeIds == null ? true : !excludeIds.Contains(row.Row.Id) ? true : false;");
                sw.WriteLine("                    if (flag)");
                sw.WriteLine("                    {");
                sw.WriteLine("                       ret.Add($\"{row.Row.Id}-{row.Name}\", row.Row.Id);");
                sw.WriteLine("                    }");
                sw.WriteLine("                }");
                sw.WriteLine("            }");
                sw.WriteLine("            return ret;");
                sw.WriteLine("        }");


                sw.WriteLine();
                sw.WriteLine($"        public static IEnumerable GetUuidDropdown(List<string> excludeUuids = null, string packageDir = null, List<Tuple<string, string>> AdditionalOptions = null, List<string> includeUuids = null)");
                sw.WriteLine("        {");
                sw.WriteLine($"           return GetUuidDropdown<{codeOverviewName}>(excludeUuids: excludeUuids, packageDir: packageDir,AdditionalOptions:AdditionalOptions,includeUuids:includeUuids);");
                sw.WriteLine("        }");


                sw.WriteLine();
                sw.WriteLine($"        public static {codeUnityRowName} GetUnityRowByUuid(string uuid, string packageDir = null)");
                sw.WriteLine("        {");
                sw.WriteLine($"           return GetUnityRowByUuid<{codeOverviewName}, {codeUnityRowName}>(uuid);");
                sw.WriteLine("        }");


                sw.WriteLine();
                sw.WriteLine($"        public static {codeOverviewName} GetOverviewByUuid(string uuid, string packageDir = null)");
                sw.WriteLine("        {");
                sw.WriteLine($"           return GetOverviewByUuid<{codeOverviewName}>(uuid);");
                sw.WriteLine("        }");

                // sw.WriteLine();
                // sw.WriteLine($"         public override void RemoveRow(string uuid)");
                // sw.WriteLine("        {");
                // sw.WriteLine($"           var unityRow = GetUnityRowByName(uuid) as {codeUnityRowName};");
                // sw.WriteLine("            if(unityRow != null)");
                // sw.WriteLine("            {");
                // sw.WriteLine("                 Rows.Remove(unityRow);");
                // sw.WriteLine("                 AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(unityRow));");
                // sw.WriteLine("            }");
                // sw.WriteLine("        }");



                sw.WriteLine();
                sw.WriteLine($"        public override void AddRow(MetaTableUnityRow unityRow)");
                sw.WriteLine("        {");
                sw.WriteLine($"           AddRow<{codeUnityRowName}>(unityRow);");
                sw.WriteLine($"           Rows.Add(unityRow as {codeUnityRowName});");
                sw.WriteLine("        }");


                sw.WriteLine();
                sw.WriteLine($"        public override void AddBaseRow(MetaTableRow row)");
                sw.WriteLine("        {");
                sw.WriteLine($"           var unityRow = ScriptableObject.CreateInstance<{codeUnityRowName}>();");
                sw.WriteLine($"           unityRow.Row = row as {rowName};");
                sw.WriteLine($"           AddRow<{codeUnityRowName}>(unityRow);");
                sw.WriteLine($"           Rows.Add(unityRow);");
                sw.WriteLine("        }");


                sw.WriteLine();
                sw.WriteLine($"        public override void UpdateRow(string uuid, MetaTableRow baseRow)");
                sw.WriteLine("        {");
                sw.WriteLine($"           foreach (var row in Rows)");
                sw.WriteLine("            {");
                sw.WriteLine($"               if (row.Uuid.Equals(uuid))");
                sw.WriteLine("                {");
                sw.WriteLine($"                   row.Row = baseRow as {rowName};");
                sw.WriteLine($"                   row.Row.Uuid = uuid;");
                sw.WriteLine($"                   return;");
                sw.WriteLine("                }");
                sw.WriteLine("            }");
                sw.WriteLine("        }");




                sw.WriteLine();
                sw.WriteLine("        [Button(\"添加行\")]");
                sw.WriteLine($"        public void AddRow()");
                sw.WriteLine("        {");
                // sw.WriteLine($"           RefreshRows();");
                sw.WriteLine($"           var unityRow = AddRow<{codeUnityRowName}>();");
                sw.WriteLine($"           Rows.Add(unityRow);");
                sw.WriteLine("        }");


                sw.WriteLine();
                sw.WriteLine("        [Button(\"刷新行\")]");
                sw.WriteLine($"        public override void RefreshRows()");
                sw.WriteLine("        {");
                sw.WriteLine($"           Rows = RefreshRows<{codeUnityRowName}>();");
                sw.WriteLine("        }");


                sw.WriteLine();
                sw.WriteLine($"        public override void RemoveByUuid(string uuid)");
                sw.WriteLine("        {");
                sw.WriteLine($"           var unityRow = GetUnityRowByUuid(uuid) as {codeUnityRowName};");
                sw.WriteLine("            if(unityRow != null)");
                sw.WriteLine("            {");
                sw.WriteLine("                 Rows.Remove(unityRow);");
                sw.WriteLine("                 AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(unityRow));");
                sw.WriteLine("            }");
                sw.WriteLine("        }");



                sw.WriteLine("#endif");

            }), codeOverviewName, codeOverviewPath, isUseUnityEditor: true, baseClass: "MetaTableOverview", isAddCreateAssetMenu: true, assetMenuPrefix: "MetaTable");

        }

        public void GeneratorNewRowWrapper(string classBaseName, string overviewName, string unityRowName, string newRowWrapperName)
        {
            var classCustomDir = Path.Join(Config.ScriptCustomDir, classBaseName);
            var path = Path.Join(classCustomDir, $"{newRowWrapperName}.cs");
            if (!File.Exists(path))
            {
                JsonClassGenerator.GeneratorCodeString("{}", $"{Namespace}", new CSharpCodeMetaTableBaseWriter(Config.UsingNamespace),
                 newRowWrapperName, path, baseClass: $"MetaTableNewRowWrapper<{overviewName},{unityRowName}>", isTotalEditor: true, isWriteFileHeader: false);
            }
        }

        public void GeneratorDetailRowWrapper(string classBaseName, string overviewName, string unityRowName, string detailRowWrapperName)
        {
            var classCustomDir = Path.Join(Config.ScriptCustomDir, classBaseName);
            var path = Path.Join(classCustomDir, $"{detailRowWrapperName}.cs");
            if (!File.Exists(path))
            {
                JsonClassGenerator.GeneratorCodeString("{}", $"{Namespace}", new CSharpCodeMetaTableBaseWriter(Config.UsingNamespace),
                 detailRowWrapperName, path, baseClass: $"MetaTableDetailRowWrapper<{overviewName},{unityRowName}>", isTotalEditor: true, isWriteFileHeader: false);
            }
        }

        public void GeneratorRowWrapper(string classBaseName, string overviewName, string unityRowName, string newRowWrapperName)
        {
            var rowWrapperName = $"{classBaseName}RowWrapper";
            var classCustomDir = Path.Join(Config.ScriptCustomDir, classBaseName);
            var rowWrapperPath = Path.Join(classCustomDir, $"{rowWrapperName}.cs");
            if (!File.Exists(rowWrapperPath))
            {
                JsonClassGenerator.GeneratorCodeString("{}", $"{Namespace}", new CSharpCodeMetaTableBaseWriter(Config.UsingNamespace),
                 rowWrapperName, rowWrapperPath, baseClass: $"MetaTableRowWrapper<{overviewName},{newRowWrapperName},{unityRowName}>", isTotalEditor: true, isWriteFileHeader: false);
            }
        }

        public void GeneratorOverviewWrapper(string classBaseName, string overviewName, string unityRowName)
        {
            var classCustomDir = Path.Join(Config.ScriptCustomDir, classBaseName);
            var overviewWrapperName = $"{classBaseName}OverviewWrapper";
            var overviewWrapperPath = Path.Join(classCustomDir, $"{overviewWrapperName}.cs");
            if (!File.Exists(overviewWrapperPath))
            {
                JsonClassGenerator.GeneratorCodeString("{}", $"{Namespace}", new CSharpCodeMetaTableBaseWriter(Config.UsingNamespace),
                 overviewWrapperName, overviewWrapperPath, baseClass: $"MetaTableOverviewWrapper<{overviewName},{classBaseName}DetailRowWrapper,{classBaseName}RowWrapper,{classBaseName}NewRowWrapper,{unityRowName}>", isTotalEditor: true, isWriteFileHeader: false);
            }
        }
        #endregion


        [Button("生成Overview")]
        [BoxGroup("基本信息/操作")]
        public void GenerateOverview()
        {
            if (MetaTableType == MetaTableTypeEnum.Define)
            {
                var path = Path.Join(Config.StreamResScriptableObjectDir, $"{OverviewName}.asset");
                if (!File.Exists(path))
                {
                    var overview = ScriptableObject.CreateInstance($"{Namespace}.{OverviewName}") as MetaTableOverview;
                    if (overview != null)
                    {
                        overview.Config = Config;
                        AssetDatabase.CreateAsset(overview, path);
                        AssetDatabase.Refresh();
                    }
                }

            }
            else if (MetaTableType == MetaTableTypeEnum.Reference)
            {
                if (RefConfig != null && !RefTableName.IsNullOrWhiteSpace())
                {
                    var RefOverviewName = $"{RefTableName}Overview";
                    var path = Path.Join(Config.StreamResScriptableObjectDir, $"{RefOverviewName}.asset");
                    if (!File.Exists(path))
                    {
                        var overview = ScriptableObject.CreateInstance($"{RefConfig.Namespace}.{RefOverviewName}") as MetaTableOverview;
                        if (overview != null)
                        {
                            overview.Config = Config;
                            AssetDatabase.CreateAsset(overview, path);
                            AssetDatabase.Refresh();
                        }
                    }

                }
            }
        }

        [Button("从Excel刷新Overview")]
        [BoxGroup("基本信息/操作")]
        public void RefreshOverview()
        {
            // Config.StreamResExcelDir
            if (RefConfig != null && !RefTableName.IsNullOrWhiteSpace())
            {
                var RefOverviewName = $"{RefTableName}Overview";
                var RefRowName = $"{RefTableName}Row";

                var path = Path.Join(Config.StreamResScriptableObjectDir, $"{RefOverviewName}.asset");

                if (File.Exists(path))
                {
                    var overviewTypeFullName = $"{RefConfig.Namespace}.{RefOverviewName}";
                    var overviewType = AssemblyUtility.GetType(overviewTypeFullName);
                    if (overviewType == null)
                    {
                        Debug.Log($"overviewType is null:{overviewTypeFullName}");
                        return;
                    }
                    MetaTableOverview overview = AssetDatabase.LoadAssetAtPath(path, overviewType) as MetaTableOverview;
                    if (overview == null)
                    {
                        Debug.Log($"overview is null. at:{path}");
                        return;
                    }

                    var excelPath = Path.Join(Config.StreamResExcelDir, $"{RefTableName}.xlsx").PathReplace();
                    if (!File.Exists(excelPath))
                    {
                        Debug.Log($"excelPath is not Exists :{excelPath}");
                        return;
                    }
                    var rowTypeFullName = $"{RefConfig.Namespace}.{RefRowName}";
                    var rowType = AssemblyUtility.GetType(rowTypeFullName);
                    if (rowType == null)
                    {
                        Debug.Log($"rowType is null:{rowTypeFullName}");
                        return;
                    }

                    var rows = ExcelHelper.LoadFromExcelFile(excelPath, rowType);
                    Debug.Log($"rows:{rows} count:{rows.Count}");

                    foreach (var row in rows)
                    {
                        var unityRow = overview.GetUnityRowByUuid(row.Uuid);
                        if (unityRow != null)
                        {
                            Debug.Log($"Update uuid:{row.Uuid} BaseRow:{row}");
                            overview.UpdateRow(row.Uuid, row);
                        }
                        else
                        {
                            Debug.Log($"Add BaseRow:{row}");
                            overview.AddBaseRow(row);
                        }

                    }

                    EditorUtility.SetDirty(overview);
                    AssetDatabase.SaveAssets();

                }
            }
        }

        [Button("从Excel刷新Overview检测Name")]
        [BoxGroup("基本信息/操作")]
        public void RefreshOverviewByName(bool onlyAdd = false)
        {
            // Config.StreamResExcelDir
            if (RefConfig != null && !RefTableName.IsNullOrWhiteSpace())
            {
                var RefOverviewName = $"{RefTableName}Overview";
                var RefRowName = $"{RefTableName}Row";

                var path = Path.Join(Config.StreamResScriptableObjectDir, $"{RefOverviewName}.asset");

                if (File.Exists(path))
                {
                    var overviewTypeFullName = $"{RefConfig.Namespace}.{RefOverviewName}";
                    var overviewType = AssemblyUtility.GetType(overviewTypeFullName);
                    if (overviewType == null)
                    {
                        Debug.Log($"overviewType is null:{overviewTypeFullName}");
                        return;
                    }
                    MetaTableOverview overview = AssetDatabase.LoadAssetAtPath(path, overviewType) as MetaTableOverview;
                    if (overview == null)
                    {
                        Debug.Log($"overview is null. at:{path}");
                        return;
                    }

                    var excelPath = Path.Join(Config.StreamResExcelDir, $"{RefTableName}.xlsx").PathReplace();
                    if (!File.Exists(excelPath))
                    {
                        Debug.Log($"excelPath is not Exists :{excelPath}");
                        return;
                    }
                    var rowTypeFullName = $"{RefConfig.Namespace}.{RefRowName}";
                    var rowType = AssemblyUtility.GetType(rowTypeFullName);
                    if (rowType == null)
                    {
                        Debug.Log($"rowType is null:{rowTypeFullName}");
                        return;
                    }

                    var rows = ExcelHelper.LoadFromExcelFile(excelPath, rowType);
                    Debug.Log($"rows:{rows} count:{rows.Count}");

                    foreach (var row in rows)
                    {

                        var overviewRow = overview.GetBaseRowByName(row.Name);
                        if (overviewRow == null)
                        {
                            Debug.Log($"Add BaseRow:{row}");
                            overview.AddBaseRow(row);
                        }
                        else
                        {
                            if (!onlyAdd)
                            {
                                Debug.Log($"Update uuid:{overviewRow.Uuid} BaseRow:{row}");
                                overview.UpdateRow(overviewRow.Uuid, row);
                            }
                        }

                    }
                    EditorUtility.SetDirty(overview);
                    AssetDatabase.SaveAssets();

                }
            }
        }
        [Button("去掉重复Name")]
        [BoxGroup("基本信息/操作")]
        public void DropDupName()
        {
            if (RefConfig != null && !RefTableName.IsNullOrWhiteSpace())
            {
                var RefOverviewName = $"{RefTableName}Overview";
                var RefRowName = $"{RefTableName}Row";

                var path = Path.Join(Config.StreamResScriptableObjectDir, $"{RefOverviewName}.asset");

                if (File.Exists(path))
                {
                    var overviewTypeFullName = $"{RefConfig.Namespace}.{RefOverviewName}";
                    var overviewType = AssemblyUtility.GetType(overviewTypeFullName);
                    if (overviewType == null)
                    {
                        Debug.Log($"overviewType is null:{overviewTypeFullName}");
                        return;
                    }
                    MetaTableOverview overview = AssetDatabase.LoadAssetAtPath(path, overviewType) as MetaTableOverview;
                    if (overview == null)
                    {
                        Debug.Log($"overview is null. at:{path}");
                        return;
                    }
                    List<string> Names = new List<string>();

                    foreach (var unityRow in overview.BaseRows)
                    {
                        if (!Names.Contains(unityRow.Name))
                        {
                            Names.Add(unityRow.Name);
                        }
                        else
                        {
                            overview.RemoveByUuid(unityRow.Uuid);
                        }
                    }
                    EditorUtility.SetDirty(overview);
                    AssetDatabase.SaveAssets();
                }
            }
        }
#endif

    }
}

