using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;


#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace MetaTable
{
    public abstract class MetaTableOverview : ScriptableObject
    {
        [ShowInInspector]
        public MetaTableConfig Config;


        [ShowInInspector]
        public string Namespace
        {
            get
            {
                if (Config != null)
                {
                    return Config.Namespace;
                }
                return null;
            }
        }

        public abstract string TableName { get; }

        public virtual string RowDirPath
        {
            get
            {
                return Path.Join(Config.StreamResScriptableObjectDir, TableName).PathReplace();
            }
        }

        public string RowPath(string filename)
        {
            return Path.Join(RowDirPath, $"{filename}.asset");
        }

        public abstract IReadOnlyList<MetaTableUnityRow> UnityBaseRows { get; }

        public IReadOnlyList<MetaTableRow> BaseRows
        {
            get
            {
                List<MetaTableRow> BaseRows = new List<MetaTableRow>();
                for (int i = 0; i < UnityBaseRows.Count; i++)
                {
                    var baseRow = UnityBaseRows[i]?.BaseRow;
                    if (baseRow != null)
                    {
                        BaseRows.Add(baseRow);
                    }

                }
                return BaseRows;
            }
        }

        public T ToTable<T>() where T : MetaTableBase, new()
        {
            T table = new T();
            table.AddRows(BaseRows);
            return table;
        }

        [Button("生成运行时表")]
        public abstract MetaTableBase ToTable();

        [Button("AddRow")]
        public T AddRow<T>() where T : MetaTableUnityRow
        {
            var unityRow = ScriptableObject.CreateInstance<T>();
            var newUuid = UuidUtility.GetNewUuid();
            unityRow.BaseRow.Uuid = newUuid;
            DirectoryUtility.ExistsOrCreate(RowDirPath);
            string dest = RowPath(newUuid);
            AssetDatabase.CreateAsset(unityRow, dest);
            AssetDatabase.SaveAssets();
            return unityRow;
        }


#if UNITY_EDITOR

        public abstract void RefreshRows();

        public List<T> RefreshRows<T>() where T : MetaTableUnityRow
        {
            List<T> ret = new List<T>();
            var rowList = AssetDatabaseUtility.FindAsset<T>(RowDirPath);
            Debug.Log($"rowList:{rowList}, {rowList?.Count()}");
            foreach (var row in rowList)
            {
                ret.Add(row as T);
            }
            return ret;
        }
#endif
    }
}

