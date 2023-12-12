// 本文件使用工具自动生成，请勿进行手动修改！

using System;
using System.IO;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Xml.Serialization;
using Pangoo.Common;

namespace MetaTable
{
    [Serializable]
        [CreateAssetMenu(fileName = "UnityAssetGroupRow", menuName = "MetaTable/UnityAssetGroupRow")]
    public partial class UnityAssetGroupRow : MetaTableUnityRow
    {

        [HideLabel]
        public AssetGroupRow Row = new();

        public override MetaTableRow BaseRow => Row;

#if UNITY_EDITOR

        public override void SetRow(MetaTableRow row)
        {
           Row = row as AssetGroupRow;
        }

        public override MetaTableRow CloneRow()
        {
           return CopyUtility.Clone<AssetGroupRow>(Row);
        }
#endif
    }
}

