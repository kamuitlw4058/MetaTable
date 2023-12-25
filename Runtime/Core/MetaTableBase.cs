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


        public abstract IMetaTableRow GetMetaTableRowByUuid(string uuid);

        public abstract IMetaTableRow GetMetaTableRowById(int id);


        public abstract IReadOnlyList<IMetaTableRow> BaseRows { get; }


        public abstract void AddRows(IReadOnlyList<IMetaTableRow> rows);



        public abstract void MergeRows(IReadOnlyList<IMetaTableRow> rows);




    }

}
