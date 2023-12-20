using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using Sirenix.Utilities;
using System.Linq;

#if UNITY_EDITOR
using System.Reflection;
using OfficeOpenXml;
using UnityEditor;
#endif

using Sirenix.OdinInspector;
using UnityEngine;

namespace MetaTable
{
    [Serializable]
    public abstract partial class MetaTableBase : IMetaTableBase
    {
        public int Priority = 0;

        public abstract string TableName { get; }


        public IMetaTableRow GetRowByUuid(string uuid)
        {
            if (Dict.TryGetValue(uuid, out IMetaTableRow row))
            {
                return row;
            }
            return null;
        }

        public IMetaTableRow GetRowById(int id)
        {
            var Values = Dict.Values.ToArray();
            for (int i = 0; i < Dict.Count; i++)
            {
                if (Values[i].Id == id)
                {
                    return Values[i];
                }
            }
            return null;
        }



        [ShowInInspector]
        public Dictionary<string, IMetaTableRow> Dict = new Dictionary<string, IMetaTableRow>();


        public IReadOnlyList<IMetaTableRow> BaseRows => Dict.Values.ToList();

        Dictionary<string, IMetaTableRow> IMetaTableBase.Dict { get => Dict; set => Dict = value; }

        public void AddRows(IReadOnlyList<IMetaTableRow> rows)
        {
            rows.ForEach((o) =>
            {
                if (o.Uuid == null)
                {
                    Debug.LogError($"AddRows Uuid Is Null");
                    return;
                }

                if (Dict.ContainsKey(o.Uuid))
                {
                    Debug.LogError($"{GetType().Name} Uuid:{o.Uuid} Dup! Please Check");
                    return;
                }
                Dict.Add(o.Uuid, o);
            });
        }


        public void MergeRows(IReadOnlyList<IMetaTableRow> rows)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                if (!Dict.ContainsKey(rows[i].Uuid))
                {
                    Dict.Add(rows[i].Uuid, rows[i]);
                }
            }
        }

        public R GetRowByUuid<R>(string uuid) where R : class, IMetaTableRow
        {
            if (Dict.TryGetValue(uuid, out IMetaTableRow row))
            {
                return row as R;
            }
            return null;
        }

        public R GetRowById<R>(int id) where R : class, IMetaTableRow
        {
            var Values = Dict.Values.ToArray();
            for (int i = 0; i < Dict.Count; i++)
            {
                if (Values[i].Id == id)
                {
                    return Values[i] as R;
                }
            }
            return null;
        }


    }

}
