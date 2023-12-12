#if UNITY_EDITOR
using System;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using Pangoo.Common;
using Pangoo;
using UnityEngine;

namespace MetaTable
{
    public class MetaTableRowWrapper<TOverview, TNewRowWrapper, TRow> : MetaTableWrapperBase<TOverview, TRow>
             where TOverview : MetaTableOverview
             where TNewRowWrapper : MetaTableNewRowWrapper<TOverview, TRow>, new()
             where TRow : MetaTableUnityRow, new()
    {


        [field: NonSerialized]
        public Action<string> OnRemove;


        private static OdinEditorWindow m_CreateWindow;


        [Button("复制")]
        [TableColumnWidth(60, resizable: false)]
        // [ShowIf("@this.ShowEditor")]
        public void Copy()
        {
            var newWrapper = new TNewRowWrapper();
            newWrapper.Overview = Overview;
            newWrapper.UnityRow = ScriptableObject.CreateInstance<TRow>();
            newWrapper.UnityRow.SetRow(UnityRow.CloneRow());
            newWrapper.UnityRow.BaseRow.Uuid = UuidUtility.GetNewUuid();
            newWrapper.AfterCreate = OnAfterCreate;
            m_CreateWindow = OdinEditorWindow.InspectObject(newWrapper);

        }

        void OnAfterCreate(string uuid)
        {
            if (m_CreateWindow != null)
            {
                m_CreateWindow.Close();
                m_CreateWindow = null;
            }
        }


        [Button("编辑")]
        [TableColumnWidth(60, resizable: false)]
        // [ShowIf("@this.ShowEditor")]
        public void Editor()
        {
            MenuWindow?.TrySelectMenuItemWithObject(UnityRow);
        }


        [Button("删除")]
        [TableColumnWidth(60, resizable: false)]
        public void Remove()
        {
            if (UnityRow == null || Uuid.IsNullOrWhiteSpace()) return;

            if (OnRemove != null)
            {
                OnRemove(Uuid);
            }

            Overview?.RemoveByUuid(Uuid);
            var path = AssetDatabase.GetAssetPath(UnityRow);
            AssetDatabase.DeleteAsset(path);
            EditorUtility.SetDirty(Overview);
            AssetDatabase.SaveAssets();

        }


    }
}

#endif