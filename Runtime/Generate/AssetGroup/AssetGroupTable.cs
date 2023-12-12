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
    public partial class AssetGroupTable : MetaTableBase
    {

        public AssetGroupRow GetRowByUuid(string uuid)
        {
            return GetRowByUuid<AssetGroupRow>(uuid);
        }
    }
}

