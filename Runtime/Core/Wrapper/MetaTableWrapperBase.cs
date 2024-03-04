#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using Pangoo.Common;
using UnityEngine;
using System.Collections.Generic;

namespace MetaTable
{
    public class MetaTableWrapperBase<TOverview, TRow> where TOverview : MetaTableOverview where TRow : MetaTableUnityRow, new()
    {
        OdinMenuEditorWindow m_MenuWindow;

        public OdinMenuEditorWindow MenuWindow
        {
            get
            {
                return m_MenuWindow;
            }
            set
            {
                m_MenuWindow = value;
            }
        }

        public bool OutsideNeedRefresh { get; set; }
        public virtual bool CanNameChange
        {
            get
            {
                return false;
            }
        }


        protected TOverview m_Overview;

        public bool EnableEditOverview { get; set; }

        [EnableIf("EnableEditOverview")]
        [TableColumnWidth(100, resizable: false)]
        [TableTitleGroup("命名空间")]
        [PropertyOrder(-4)]
        [HideLabel]
        [ValueDropdown("OnTOverviewDropdown")]
        [ShowInInspector]
        public virtual TOverview Overview
        {
            get
            {
                return m_Overview;
            }
            set
            {
                m_Overview = value;
            }
        }

        public IEnumerable OnTOverviewDropdown()
        {
            return MetaTableOverview.GetOverviewDropdown<TOverview>();
        }

        protected TRow m_UnityRow;

        [ShowInInspector]
        [TableTitleGroup("引用")]
        [TableColumnWidth(80, resizable: false)]
        [PropertyOrder(-3)]
        [ReadOnly]
        [HideLabel]

        public TRow UnityRow
        {
            get
            {
                return m_UnityRow;
            }
            set
            {
                m_UnityRow = value;
            }
        }


        [ShowInInspector]
        [TableColumnWidth(120, resizable: false)]
        [PropertyOrder(-2)]
        [PropertyTooltip("$Uuid")]
        [DelayedProperty]
        [InfoBox("已经有对应的Uuid", InfoMessageType.Warning, "CheckExistsUuid")]

        public virtual string Uuid
        {
            get
            {
                // Debug.Log($"Log:{m_Row}");
                return m_UnityRow?.BaseRow?.Uuid;
            }
        }

        public string UuidShort
        {
            get
            {
                // Debug.Log($"Log:{m_Row}");
                return m_UnityRow?.BaseRow?.UuidShort;
            }
        }


        // [ShowInInspector]
        // [TableColumnWidth(80, resizable: false)]
        // [PropertyOrder(-2)]
        // [DelayedProperty]
        // // [InfoBox("已经有对应的Uuid", InfoMessageType.Warning, "CheckExistsUuid")]

        // public virtual int Id
        // {
        //     get
        //     {
        //         // Debug.Log($"Log:{m_Row}");
        //         return m_UnityRow.BaseRow.Id;
        //     }
        // }



        [ShowInInspector]
        [PropertyOrder(-1)]
        [EnableIf("CanNameChange")]
        [DelayedProperty]
        // [TableTitleGroup("名字")]
        [InfoBox("已经有对应的名字", InfoMessageType.Warning, "CheckExistsName")]
        [OnValueChanged("OnNameChanged")]
        public virtual string Name
        {
            get
            {
                return m_UnityRow?.BaseRow?.Name;
            }
            set
            {
                if (m_UnityRow?.BaseRow != null)
                {
                    m_UnityRow.BaseRow.Name = value;
                }

            }
        }

        public virtual void OnNameChanged()
        {

        }

#if UNITY_EDITOR

        public virtual void Save()
        {
            EditorUtility.SetDirty(m_Overview);
            EditorUtility.SetDirty(m_UnityRow);
            AssetDatabase.SaveAssets();

            OutsideNeedRefresh = true;
        }
#endif

        protected virtual bool CheckExistsUuid()
        {
            return false;
        }



        protected virtual bool CheckExistsName()
        {
            return false;
        }

    }
}

#endif