#if UNITY_EDITOR

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
    public partial class AssetGroupRowWrapper : MetaTableRowWrapper<AssetGroupOverview,AssetGroupNewRowWrapper,UnityAssetGroupRow>
    {

    }
}
#endif

