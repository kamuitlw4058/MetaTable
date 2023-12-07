using System.Collections.Generic;
using System.IO;
using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Sirenix.Utilities;



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



        // [FormerlySerializedAs("Headers")]
        // [HideInTables]
        // public List<string> UsingNamespace = new List<string>()
        // {
        //     "System",
        //     "System.IO",
        //     "System.Collections.Generic",
        //     "LitJson",
        //     // "Pangoo",
        //     "UnityEngine",
        //     "Sirenix.OdinInspector",
        //     "System.Xml.Serialization"
        // };


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


            // var classBaseName = JsonClassGenerator.ToTitleCase($"{ExcelName}");
            // ExcelTableData ExcelData = ExcelTableData.ParserEPPlus(excelFilePath, classBaseName);
            // Debug.Log($"excelFilePath:{excelFilePath},classBaseName:{classBaseName},ExcelData:{ExcelData}");
            // Debug.Log($"ExcelData:{ExcelData.ColInfnDict}");
            // Columns = ExcelData.ColInfnDict.Values.ToList();
        }

        // [Button("生成脚本")]
        // [BoxGroup("基本信息/操作")]
        // public void GeneratorCode()
        // {
        //     if (Columns.Count == 0)
        //     {
        //         return;
        //     }


        //     // var excelFilePath = Path.Join(DirInfo.ExcelDir, $"{ExcelName}.xlsx").Replace("\\", "/");
        //     // Debug.Log($"Start Build:{excelFilePath}");

        //     var classBaseName = JsonClassGenerator.ToTitleCase(ExcelName);
        //     var classRowName = $"{classBaseName}Row";
        //     var classDir = Path.Join(DirInfo.ScriptGenerateDir, classBaseName);
        //     DirectoryUtility.ExistsOrCreate(classDir);


        //     // var className = JsonClassGenerator.ToTitleCase($"{ExcelName}Table");
        //     // ExcelTableData ExcelData = ExcelTableData.ParserEPPlus(excelFilePath, classBaseName);

        //     var rowJson = DataTableCodeGenerator.BuildCodeRowJson(Columns);
        //     Debug.Log($"rowJson:{rowJson}");
        //     if (rowJson != null)
        //     {
        //         var codeRowPath = Path.Join(classDir, $"{classRowName}.cs");
        //         JsonClassGenerator.GeneratorCodeString(rowJson, DirInfo.NameSpace, new CSharpCodeRowWriter(UsingNamespace, Columns, Named), classRowName, codeRowPath);
        //     }

        //     var codeTableName = $"{classBaseName}RTTable";
        //     var codeTablePath = Path.Join(classDir, $"{codeTableName}.cs");
        //     JsonClassGenerator.GeneratorCodeString("{}", DirInfo.NameSpace, new CSharpCodeRTTableWriter(UsingNamespace, Named, classRowName), codeTableName, codeTablePath);


        //     var codeUnityRow = $"Unity{classBaseName}Row";
        //     var codeUnityRowPath = Path.Join(classDir, $"{codeUnityRow}.cs");
        //     JsonClassGenerator.GeneratorCodeString("{}", DirInfo.NameSpace, new CSharpCodeUnityRowWriter(UsingNamespace, Named, classRowName), codeUnityRow, codeUnityRowPath);


        //     var codeUnityRowCustomPath = Path.Join(classDir, $"{codeUnityRow}.Custom.cs");
        //     if (!File.Exists(codeUnityRowCustomPath))
        //     {
        //         JsonClassGenerator.GeneratorCodeString("{}", DirInfo.NameSpace, new CSharpCodeUnityRowCustomWriter(UsingNamespace, Named, classRowName), codeUnityRow, codeUnityRowCustomPath);
        //     }


        //     var codeOverviewName = $"{classBaseName}RTOverview";
        //     var codeOverviewPath = Path.Join(classDir, $"{codeOverviewName}.cs");
        //     JsonClassGenerator.GeneratorCodeString("{}", DirInfo.NameSpace, new CSharpCodeTableRTOverviewWriter(UsingNamespace, classBaseName, codeUnityRow, codeTableName), codeOverviewName, codeOverviewPath);



        //     AssetDatabase.Refresh();


        // }
    }
}

