
using System.IO;
using Sirenix.OdinInspector;

namespace MetaTable
{
    public partial class MetaTableConfig
    {
        const string MetaTableName = "MetaTable";

        [ShowInInspector]
        public string ScriptMetaDir
        {
            get
            {
                return Path.Join(PackageDir, ScriptDir).PathReplace();
            }
        }

        [ShowInInspector]
        public string ScriptGenerateDir
        {
            get
            {
                return Path.Join(ScriptMetaDir, MetaTableName, "Generate").PathReplace();
            }
        }

        [ShowInInspector]
        public string ScriptCustomDir
        {
            get
            {
                return Path.Join(ScriptMetaDir, MetaTableName, "Custom").PathReplace();
            }
        }


        [ShowInInspector]
        public string StreamResMetaDir
        {
            get
            {
                return Path.Join(PackageDir, StreamResDir, MetaTableName).PathReplace();
            }
        }

        [ShowInInspector]
        public string StreamResExcelDir
        {
            get
            {
                return Path.Join(StreamResMetaDir, "Excel").PathReplace();
            }
        }


        [ShowInInspector]
        public string StreamResScriptableObjectDir
        {
            get
            {
                return Path.Join(StreamResMetaDir, "ScriptableObject").PathReplace();
            }
        }
    }
}

