using System.Collections.Generic;
using System.IO;
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

    [CreateAssetMenu(fileName = "MetaTableConfig", menuName = "MetaTable/Config", order = 0)]
    public partial class MetaTableConfig : ScriptableObject
    {

        [LabelText("命名空间")]
        public string Namespace;

        [LabelText("包路径")]
        [InlineButton("GuessPackageDir", "猜测包路径")]
        public string PackageDir;


        [LabelText("脚本目录")]
        public string ScriptDir = "Runtime";

        [LabelText("资源包目录")]
        public string StreamResDir = "StreamRes";


        public void GuessPackageDir()
        {
            var path = AssetDatabase.GetAssetPath(this);
            if (path.IsNullOrWhiteSpace())
            {
                return;
            }
            Debug.Log($"Guess Path:{path}");
            PackageDir = Path.GetDirectoryName(path).PathReplace();
            var parentDir = PathUtility.MatchParentDirs(PackageDir, new string[] { "Configs", "StreamRes" });
            if (parentDir != null)
            {
                PackageDir = parentDir;
            }

            PackageDir = PackageDir.PathReplace();
        }

        public void GuessScriptDir()
        {
            if (!PackageDir.IsNullOrWhiteSpace())
            {

            }
        }

        [FormerlySerializedAs("Headers")]
        [LabelText("引用头文件")]
        public List<string> UsingNamespace = new List<string>()
        {
            "System",
            "System.IO",
            "System.Collections.Generic",
            "LitJson",
            "UnityEngine",
            "Sirenix.OdinInspector",
            "System.Xml.Serialization",
            "Pangoo.Common",
            "MetaTable"
        };

        [TableList(AlwaysExpanded = true)]
        [OnCollectionChanged("AfterChanged")]

        public List<MetaTableEntry> Entries = new List<MetaTableEntry>();


        public void AfterChanged()
        {
            foreach (var entry in Entries)
            {
                // if (entry.Namespace.IsNullOrWhiteSpace())
                // {
                //     entry.Namespace = Namespace;
                // }

                entry.Config = this;
            }
        }

        [Button("初始化目录")]
        public void InitDirs()
        {
            DirectoryUtility.ExistsOrCreate(ScriptCustomDir);
            DirectoryUtility.ExistsOrCreate(ScriptGenerateDir);

            DirectoryUtility.ExistsOrCreate(StreamResExcelDir);
            DirectoryUtility.ExistsOrCreate(StreamResScriptableObjectDir);

        }


        // [Button("刷新Excel列表", 30)]
        // void Refresh()
        // {
        //     if (PackConfig == null)
        //     {
        //         Debug.LogError("Load Config Failed!");
        //         return;
        //     }

        //     InitDirInfo();
        //     ExcelList.Clear();
        //     DirInfo.NameSpace = PackConfig.MainNamespace;
        //     var files = Directory.GetFiles(DirInfo.ExcelDir, "*.xlsx");
        //     var pangooList = new List<string>();
        //     foreach (var filePath in files)
        //     {
        //         var regularFilePath = filePath.Replace("\\", "/");
        //         var fileName = Path.GetFileNameWithoutExtension(regularFilePath);
        //         if (!fileName.StartsWith("~"))
        //         {
        //             if (ExcelList.Find(o => o.ExcelName == fileName) == null)
        //             {
        //                 var namesapce = string.Empty;
        //                 var IsPangooTable = false;
        //                 if (GameSupportEditorUtility.GetExcelTableNameInPangoo(fileName) && PackConfig.MainNamespace != "Pangoo")
        //                 {
        //                     namesapce = "Pangoo";
        //                     IsPangooTable = true;
        //                     pangooList.Add(GameSupportEditorUtility.GetExcelTablePangooTableName(fileName));
        //                 }

        //                 if (PackConfig.MainNamespace == "Pangoo")
        //                 {
        //                     IsPangooTable = true;
        //                 }

        //                 ExcelList.Add(new ExcelEntry()
        //                 {
        //                     ExcelName = fileName,
        //                     BaseNamespace = namesapce,
        //                     IsPangooTable = IsPangooTable,
        //                     Named = true,
        //                 });
        //             }

        //         }
        //     }

        //     if (PackConfig.MainNamespace != "Pangoo")
        //     {
        //         GameSupportEditorUtility.GetExcelTablePangooTableNames().ForEach(o =>
        //         {
        //             if (!pangooList.Contains(o))
        //             {
        //                 ExcelList.Add(new ExcelEntry()
        //                 {
        //                     ExcelName = o.Substring(7, o.Length - (5 + 7)),
        //                     BaseNamespace = "Pangoo",
        //                     IsPangooTable = true,
        //                     Named = true,
        //                 });
        //             }
        //         });
        //     }




        // }


        [Button("从Excel刷新列头")]
        public void UpdateColumnsByExcel(bool replaceId2Uuid = false)
        {
            var files = Directory.GetFiles(StreamResExcelDir, "*.xlsx");
            var pangooList = new List<string>();
            foreach (var filePath in files)
            {
                var regularFilePath = filePath.PathReplace();
                var fileName = Path.GetFileNameWithoutExtension(regularFilePath);
                if (!fileName.StartsWith("~"))
                {
                    if (Entries.Find(o => o.TableName == fileName) == null)
                    {
                        var entry = new MetaTableEntry()
                        {
                            TableName = fileName,
                            Config = this,
                        };
                        entry.UpdateColumnsByExcel(replaceId2Uuid);
                        Entries.Add(entry);
                    }
                }
            }


            // if (TableName.IsNullOrWhiteSpace() || Config == null)
            // {
            //     return;
            // }


            // TableName = NameUtility.ToTitleCase(TableName);



            // var excelFilePath = Path.Join(Config.StreamResExcelDir, $"{TableName}.xlsx").PathReplace();
            // if (!File.Exists(excelFilePath))
            // {
            //     return;
            // }

            // var excelColumns = ExcelHelper.ParserEPPlus(excelFilePath);
            // if (excelColumns != null)
            // {
            //     Columns = excelColumns;
            // }
        }

#if UNITY_EDITOR
        [Button("保存配置", 30)]
        public void SaveConfig()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif
    }
}

