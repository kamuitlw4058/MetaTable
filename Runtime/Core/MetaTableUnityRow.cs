using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MetaTable
{
    public abstract class MetaTableUnityRow : ScriptableObject
    {

        public abstract MetaTableRow BaseRow { get; }

#if UNITY_EDITOR
        public abstract void SetRow(MetaTableRow row);

        public abstract MetaTableRow CloneRow();
#endif

    }

}
