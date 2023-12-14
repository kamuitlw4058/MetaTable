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
using System.IO;
using Object = UnityEngine.Object;

namespace MetaTable
{
    [Serializable]
    public abstract partial class MetaTableBase
    {
        public int Priority = 0;

        [ShowInInspector]
        public Dictionary<string, MetaTableRow> Dict = new Dictionary<string, MetaTableRow>();


        public IReadOnlyList<MetaTableRow> BaseRows => Dict.Values.ToList();


        public void AddRows(IReadOnlyList<MetaTableRow> rows)
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

        public T GetRowByUuid<T>(string uuid) where T : MetaTableRow
        {
            if (Dict.TryGetValue(uuid, out MetaTableRow row))
            {
                return row as T;
            }
            return null;
        }

    }

}
