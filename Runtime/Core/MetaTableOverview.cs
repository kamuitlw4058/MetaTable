using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using System.IO;
using System.Collections;
using UnityEngine.PlayerLoop;
using System.Data;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;




#if UNITY_EDITOR

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

        public MetaTableRow GetBaseRowById(int id)
        {
            if (id == 0)
            {
                return null;
            }
            foreach (var unityRow in UnityBaseRows)
            {
                if (id.Equals(unityRow.BaseRow.Id))
                {
                    return unityRow.BaseRow;
                }
            }
            return null;
        }

        public MetaTableRow GetBaseRowByName(string name)
        {
            if (name.IsNullOrWhiteSpace())
            {
                return null;
            }
            foreach (var unityRow in UnityBaseRows)
            {
                if (name.Equals(unityRow.Name))
                {
                    return unityRow.BaseRow;
                }
            }
            return null;
        }

        public MetaTableRow GetBaseRowByUuid(string uuid)
        {
            if (uuid.IsNullOrWhiteSpace())
            {
                return null;
            }
            foreach (var unityRow in UnityBaseRows)
            {
                if (uuid.Equals(unityRow.Uuid))
                {
                    return unityRow.BaseRow;
                }
            }
            return null;
        }

        public MetaTableUnityRow GetUnityRowByName(string name)
        {
            if (name.IsNullOrWhiteSpace())
            {
                return null;
            }
            foreach (var unityRow in UnityBaseRows)
            {
                if (name.Equals(unityRow.Name))
                {
                    return unityRow;
                }
            }
            return null;
        }

        public MetaTableUnityRow GetUnityRowByUuid(string uuid)
        {
            if (uuid.IsNullOrWhiteSpace())
            {
                return null;
            }
            foreach (var unityRow in UnityBaseRows)
            {
                if (uuid.Equals(unityRow.Uuid))
                {
                    return unityRow;
                }
            }
            return null;
        }

        public T ToTable<T>() where T : MetaTableBase, new()
        {
            T table = new T();
            table.AddRows(BaseRows);
            return table;
        }

        [Button("生成运行时表")]
        public abstract MetaTableBase ToTable();


#if UNITY_EDITOR
        // public abstract void RemoveRow(string uuid);

        public abstract void RemoveByUuid(string uuid);

        public abstract void AddRow(MetaTableUnityRow unityRow);

        public abstract void AddBaseRow(MetaTableRow row);

        public abstract void UpdateRow(string uuid, MetaTableRow row);






        public static IEnumerable GetUuidDropdown<T>(List<string> excludeUuids = null, string packageDir = null) where T : MetaTableOverview
        {
            var ret = new ValueDropdownList<string>();
            var overviews = AssetDatabaseUtility.FindAsset<T>(packageDir);
            foreach (var overview in overviews)
            {
                foreach (var row in overview.BaseRows)
                {
                    bool flag = excludeUuids == null ? true : !excludeUuids.Contains(row.Uuid) ? true : false;
                    if (flag)
                    {
                        ret.Add($"{row.UuidShort}-{row.Name}", row.Uuid);
                    }
                }
            }
            return ret;
        }


        public static R GetUnityRowByUuid<T, R>(string uuid, string packageDir = null) where T : MetaTableOverview where R : MetaTableUnityRow
        {
            var overviews = AssetDatabaseUtility.FindAsset<T>(packageDir);
            foreach (var overview in overviews)
            {

                foreach (var row in overview.UnityBaseRows)
                {
                    if (row.Uuid.Equals(uuid))
                    {
                        return row as R;
                    }

                }
            }
            return null;
        }




        public T AddRow<T>(MetaTableUnityRow unityRow = null) where T : MetaTableUnityRow
        {
            if (unityRow == null)
            {
                unityRow = ScriptableObject.CreateInstance<T>();
                var newUuid = UuidUtility.GetNewUuid();
                unityRow.BaseRow.Uuid = newUuid;
            }

            DirectoryUtility.ExistsOrCreate(RowDirPath);
            string dest = RowPath(unityRow.BaseRow.Uuid);
            AssetDatabase.CreateAsset(unityRow, dest);
            AssetDatabase.SaveAssets();
            return unityRow as T;
        }


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

