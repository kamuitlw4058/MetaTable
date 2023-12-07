using System.Collections;
using System.IO;
using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Sirenix.Utilities;



#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
#endif

namespace MetaTable
{

    [Serializable]
    public class MetaTableColumn
    {

        [VerticalGroup("名字")]
        [HideLabel]
        public string Name;

        [VerticalGroup("类型")]
        [ValueDropdown("GetTypeList")]
        [HideLabel]
        public string Type;

        [VerticalGroup("中文名")]
        [HideLabel]
        public string CnName;

#if UNITY_EDITOR
        IEnumerable GetTypeList()
        {
            ValueDropdownList<string> ret = new ValueDropdownList<string>();
            ret.Add("string");
            ret.Add("int");
            ret.Add("float");
            ret.Add("double");
            ret.Add("Vector3");
            ret.Add("bool");
            return ret;
        }
#endif
    }
}

