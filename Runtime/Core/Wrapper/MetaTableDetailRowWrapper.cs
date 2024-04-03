#if UNITY_EDITOR
using Sirenix.OdinInspector;
using System.Collections;
using System.Linq;
using UnityEngine;
using System;

using UnityEditor;


namespace MetaTable
{
    public class MetaTableDetailRowWrapper<TOverview, TRow> : MetaTableWrapperBase<TOverview, TRow>
                        where TOverview : MetaTableOverview
                        where TRow : MetaTableUnityRow, new()
    {

        public override bool CanNameChange => true;

        public IMetaTableEditor Editor { get; set; }
    }
}

#endif