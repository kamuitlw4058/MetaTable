using System.Collections.Generic;
using System.IO;
using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Sirenix.Utilities;
using ClassGenerator;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;



#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
#endif

namespace MetaTable
{

    [Serializable]
    public class MetaTableEntry
    {
        [ReadOnly]
        [VerticalGroup("基本信息")]
        [LabelText("配置")]
        public MetaTableConfig Config;


        // [ReadOnly]
        [VerticalGroup("基本信息")]
        [LabelText("表名")]
        [TableColumnWidth(240, resizable: false)]

        public string TableName;

        // [ValueDropdown("GetNamespaces")]
        [VerticalGroup("基本信息")]
        [LabelText("命名空间")]
        public string Namespace;




        // IEnumerable GetNamespaces()
        // {
        //     return GameSupportEditorUtility.GetNamespaces();

        // }

        [VerticalGroup("表头编辑")]
        [LabelText("表头")]
        [TableList(AlwaysExpanded = true)]
        public List<MetaTableColumn> Columns = new List<MetaTableColumn>();

        [Button("从Excel刷新列头")]
        [BoxGroup("基本信息/操作")]
        public void UpdateColumnsByExcel()
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

            var excelColumns = ExcelHelper.ParserEPPlus(excelFilePath);
            if (excelColumns != null)
            {
                Columns = excelColumns;
            }
        }

        [Button("生成脚本")]
        [BoxGroup("基本信息/操作")]
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


            var codeRowPath = Path.Join(classGenerateDir, $"{rowName}.cs");
            JsonClassGenerator.GeneratorCodeString(rowJson, Namespace, new CSharpCodeRowWriter(Config.UsingNamespace, Columns), rowName, codeRowPath, baseClass: "MetaTableRow", baseFields: new string[] { "Uuid", "Name" });

            var codeTableName = $"{classBaseName}Table";
            GeneratorTable(codeTableName, classGenerateDir, rowName);

            var unityRowName = $"Unity{classBaseName}Row";
            GeneratorUnityRow(classBaseName, unityRowName, classGenerateDir, rowName);

            GeneratorOverview(classBaseName, classGenerateDir, unityRowName, codeTableName);

            AssetDatabase.Refresh();
        }

        public void GeneratorTable(string codeTableName, string classGenerateDir, string classRowName)
        {
            var codeTablePath = Path.Join(classGenerateDir, $"{codeTableName}.cs");
            JsonClassGenerator.GeneratorCodeString("{}", Namespace, new CSharpCodeMetaTableBaseWriter(Config.UsingNamespace, (config, sw) =>
            {
                sw.WriteLine($"        public {classRowName} GetRowByUuid(string uuid)");
                sw.WriteLine("        {");
                sw.WriteLine($"            return GetRowByUuid<{classRowName}>(uuid);");
                sw.WriteLine("        }");
            }), codeTableName, codeTablePath, baseClass: "MetaTableBase");
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
                sw.WriteLine($"           return CopyUtility.Clone<AssetGroupRow>(Row);");
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


        public void GeneratorOverview(string classBaseName, string classGenerateDir, string codeUnityRowName, string codeTableName)
        {
            var codeOverviewName = $"{classBaseName}Overview";
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
                sw.WriteLine($"        public override void AddRow(MetaTableUnityRow unityRow)");
                sw.WriteLine("        {");
                sw.WriteLine($"           AddRow<{codeUnityRowName}>(unityRow);");
                sw.WriteLine($"           Rows.Add(unityRow as {codeUnityRowName});");
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
                sw.WriteLine($"           for (int i = 0; i < Rows.Count; i++)");
                sw.WriteLine("           {");
                sw.WriteLine("               if (Rows[i].Row.Uuid.Equals(uuid)){");
                sw.WriteLine($"                   Rows.Remove(Rows[i]);");
                sw.WriteLine("               }");
                sw.WriteLine("           }");
                sw.WriteLine("        }");


                sw.WriteLine("#endif");

            }), codeOverviewName, codeOverviewPath, isUseUnityEditor: true, baseClass: "MetaTableOverview", isAddCreateAssetMenu: true, assetMenuPrefix: "MetaTable");

        }





    }
}

