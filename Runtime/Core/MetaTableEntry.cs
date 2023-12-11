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



        [FormerlySerializedAs("Headers")]
        [HideInTables]
        public List<string> UsingNamespace = new List<string>()
        {
            "System",
            "System.IO",
            "System.Collections.Generic",
            "LitJson",
            "UnityEngine",
            "Sirenix.OdinInspector",
            "System.Xml.Serialization"
        };


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


            // var excelFilePath = Path.Join(DirInfo.ExcelDir, $"{ExcelName}.xlsx").Replace("\\", "/");
            // Debug.Log($"Start Build:{excelFilePath}");

            var classBaseName = NameUtility.ToTitleCase(TableName);
            var classRowName = $"{classBaseName}Row";
            var classGenerateDir = Path.Join(Config.ScriptGenerateDir, classBaseName);
            DirectoryUtility.ExistsOrCreate(classGenerateDir);


            var classTableName = NameUtility.ToTitleCase($"{classBaseName}Table");
            // // ExcelTableData ExcelData = ExcelTableData.ParserEPPlus(excelFilePath, classBaseName);

            var rowJson = CodeGeneratorJson.BuildRowCodeJson(Columns);
            // Debug.Log($"rowJson:{rowJson}");
            if (rowJson != null)
            {
                var codeRowPath = Path.Join(classGenerateDir, $"{classRowName}.cs");
                JsonClassGenerator.GeneratorCodeString(rowJson, Namespace, new CSharpCodeRowWriter(UsingNamespace, Columns), classRowName, codeRowPath, baseClass: "MetaTableRow", baseFields: new string[] { "Uuid", "Name" });
            }

            var codeTableName = $"{classBaseName}Table";
            var codeTablePath = Path.Join(classGenerateDir, $"{codeTableName}.cs");
            JsonClassGenerator.GeneratorCodeString("{}", Namespace, new CSharpCodeMetaTableBaseWriter(UsingNamespace, (config, sw) =>
            {
                sw.WriteLine($"        public {classRowName} GetRowByUuid(string uuid)");
                sw.WriteLine("        {");
                sw.WriteLine($"            return GetRowByUuid<{classRowName}>(uuid);");
                sw.WriteLine("        }");
            }), codeTableName, codeTablePath, baseClass: "MetaTableBase");


            var codeUnityRow = $"Unity{classBaseName}Row";
            var codeUnityRowPath = Path.Join(classGenerateDir, $"{codeUnityRow}.cs");
            JsonClassGenerator.GeneratorCodeString("{}", Namespace, new CSharpCodeMetaTableBaseWriter(UsingNamespace, (config, sw) =>
            {

                sw.WriteLine($"        [HideLabel]");
                sw.WriteLine($"        public {classRowName} Row = new();");

                sw.WriteLine();
                sw.WriteLine($"         public override MetaTableRow BaseRow => Row;");

            }), codeUnityRow, codeUnityRowPath, baseClass: "MetaTableUnityRow", isAddCreateAssetMenu: true, assetMenuPrefix: "MetaTable");


            var classCustomDir = Path.Join(Config.ScriptCustomDir, classBaseName);
            DirectoryUtility.ExistsOrCreate(classCustomDir);
            var codeUnityRowCustomPath = Path.Join(classCustomDir, $"{codeUnityRow}.Custom.cs");
            if (!File.Exists(codeUnityRowCustomPath))
            {
                JsonClassGenerator.GeneratorCodeString("{}", Namespace, new CSharpCodeMetaTableBaseWriter(UsingNamespace), codeUnityRow, codeUnityRowCustomPath, isSerializable: false, isWriteFileHeader: false);
            }

            var codeOverviewName = $"{classBaseName}Overview";
            var codeOverviewPath = Path.Join(classGenerateDir, $"{codeOverviewName}.cs");
            JsonClassGenerator.GeneratorCodeString("{}", Namespace, new CSharpCodeMetaTableBaseWriter(UsingNamespace, (config, sw) =>
            {
                sw.WriteLine();

                sw.WriteLine($"        [TableList(AlwaysExpanded = true)]");
                sw.WriteLine($"        public List<{codeUnityRow}> Rows = new();");

                sw.WriteLine();
                sw.WriteLine("        [Button(\"AddRow\")]");
                sw.WriteLine($"        public void AddRow()");
                sw.WriteLine("        {");
                sw.WriteLine($"           RefreshRows();");
                sw.WriteLine($"           var unityRow = AddRow<UnityAssetGroupRow>();");
                sw.WriteLine($"           Rows.Add(unityRow);");
                sw.WriteLine("        }");


                sw.WriteLine();
                sw.WriteLine($"        public override string TableName => \"{classBaseName}\";");

                sw.WriteLine();
                sw.WriteLine($"         public override IReadOnlyList<MetaTableUnityRow> UnityBaseRows => Rows;");


                sw.WriteLine();
                sw.WriteLine($"        public override void RefreshRows()");
                sw.WriteLine("        {");
                sw.WriteLine($"           Rows = RefreshRows<{codeUnityRow}>();");
                sw.WriteLine("        }");


                sw.WriteLine();
                sw.WriteLine($"        public override MetaTableBase ToTable()");
                sw.WriteLine("        {");
                sw.WriteLine($"           return ToTable<{codeTableName}>();");
                sw.WriteLine("        }");





            }), codeOverviewName, codeOverviewPath, baseClass: "MetaTableOverview", isAddCreateAssetMenu: true, assetMenuPrefix: "MetaTable");


            AssetDatabase.Refresh();


        }





    }
}

