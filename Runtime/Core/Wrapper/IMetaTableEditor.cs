#if UNITY_EDITOR
using Sirenix.OdinInspector;
using System.Collections;
using System.Linq;
using UnityEngine;
using System;

using UnityEditor;


namespace MetaTable
{
    public interface IMetaTableEditor
    {
        void SetDetailWrapper(string uuid, object obj);

        object GetDetailWrapper(string uuid);


        void SetRowWrapper(string uuid, object obj);

        object GetRowWrapper(string uuid);

    }
}

#endif