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
        private List<string> UsingNamespace = new List<string>()
        {
            "System",
            "System.IO",
            "System.Collections.Generic",
            "LitJson",
            "UnityEngine",
            "Sirenix.OdinInspector",
            "System.Xml.Serialization"
        };

        [TableList(AlwaysExpanded = true)]
        [OnCollectionChanged("AfterChanged")]

        public List<MetaTableEntry> Entries = new List<MetaTableEntry>();


        public void AfterChanged()
        {
            foreach (var entry in Entries)
            {
                if (entry.Namespace.IsNullOrWhiteSpace())
                {
                    entry.Namespace = Namespace;
                }

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


    }
}

