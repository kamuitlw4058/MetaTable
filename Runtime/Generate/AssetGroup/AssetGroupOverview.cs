// 本文件使用工具自动生成，请勿进行手动修改！

using System;
using System.IO;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Xml.Serialization;

namespace MetaTable
{
    [Serializable]
        [CreateAssetMenu(fileName = "AssetGroupOverview", menuName = "MetaTable/AssetGroupOverview")]
    public partial class AssetGroupOverview : MetaTableOverview
    {


        [TableList(AlwaysExpanded = true)]
        public List<UnityAssetGroupRow> Rows = new();

        [Button("AddRow")]
        public void AddRow()
        {
           RefreshRows();
           var unityRow = AddRow<UnityAssetGroupRow>();
           Rows.Add(unityRow);
        }

        public override string TableName => "AssetGroup";

         public override IReadOnlyList<MetaTableUnityRow> UnityBaseRows => Rows;

        public override void RefreshRows()
        {
           Rows = RefreshRows<UnityAssetGroupRow>();
        }

        public override MetaTableBase ToTable()
        {
           return ToTable<AssetGroupTable>();
        }
    }
}

