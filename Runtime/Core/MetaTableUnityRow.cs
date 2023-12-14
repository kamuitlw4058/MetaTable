using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MetaTable
{
    public abstract class MetaTableUnityRow : ScriptableObject
    {

        public abstract MetaTableRow BaseRow { get; }

        public string Uuid
        {
            get
            {
                return BaseRow?.Uuid;
            }
        }

        public string UuidShort
        {
            get
            {
                return BaseRow?.Uuid?.Substring(0, 8);
            }
        }

        public string Name
        {
            get
            {
                return BaseRow?.Name;
            }
        }

#if UNITY_EDITOR
        public abstract void SetRow(MetaTableRow row);

        public abstract MetaTableRow CloneRow();
#endif

    }

}
