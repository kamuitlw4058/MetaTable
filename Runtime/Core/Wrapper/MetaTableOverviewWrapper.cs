#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

using MetaTable;

namespace MetaTable
{
    public class MetaTableOverviewWrapper<TOverview, TDetalRowWrapper, TTableRowWrapper, TNewRowWrapper, TRow>
            where TDetalRowWrapper : MetaTableDetailRowWrapper<TOverview, TRow>, new()
            where TTableRowWrapper : MetaTableRowWrapper<TOverview, TNewRowWrapper, TRow>, new()
            where TNewRowWrapper : MetaTableNewRowWrapper<TOverview, TRow>, new()
            where TOverview : MetaTableOverview
            where TRow : MetaTableUnityRow, new()
    {

        public IMetaTableEditor Editor { get; set; }

        List<TOverview> m_Overviews;

        // [ShowInInspector]
        public List<TOverview> Overviews
        {
            get
            {
                return m_Overviews;
            }
            set
            {
                m_Overviews = value;
            }
        }
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



        public OdinMenuTree Tree { get; set; }

        private static OdinEditorWindow m_CreateWindow;

        [Searchable]
        [TableList(IsReadOnly = true, AlwaysExpanded = true, ShowPaging = true, NumberOfItemsPerPage = 25)]
        [ShowInInspector]
        [LabelText("行数据")]
        public readonly List<TTableRowWrapper> m_AllWrappers = new List<TTableRowWrapper>();

        private Dictionary<string, OdinMenuItem> MenuItemDict = new Dictionary<string, OdinMenuItem>();

        public string MenuDisplayName { get; set; }



        public void InitWrappers()
        {
            foreach (var kv in MenuItemDict)
            {
                Tree.MenuItems.Remove(kv.Value);
            }
            MenuItemDict.Clear();
            m_AllWrappers.Clear();
            List<TTableRowWrapper> tmpRows = new List<TTableRowWrapper>();
            foreach (var overview in m_Overviews)
            {
                tmpRows.AddRange(overview.UnityBaseRows.Select(x =>
                {
                    if (x == null) return null;

                    var detailWrapper = new TDetalRowWrapper();
                    detailWrapper.Overview = overview;
                    detailWrapper.UnityRow = x as TRow;
                    detailWrapper.MenuWindow = MenuWindow;
                    detailWrapper.Editor = Editor;
                    Editor.SetDetailWrapper(detailWrapper.Uuid, detailWrapper);

                    var wrapper = new TTableRowWrapper();
                    wrapper.Overview = overview;
                    wrapper.UnityRow = x as TRow;
                    wrapper.MenuWindow = MenuWindow;
                    wrapper.DetailWrapper = detailWrapper;
                    wrapper.Editor = Editor;
                    Editor.SetRowWrapper(wrapper.Uuid, wrapper);

                    return wrapper;
                }).ToList());
            }

            tmpRows = tmpRows.Where(o => o != null).ToList();

            foreach (var wrapper in tmpRows)
            {
                var itemMenuKey = wrapper.Uuid;
                // Debug.Log($"wrapper:{wrapper}");
                var itemDisplayName = $"{MenuDisplayName}-{wrapper.UuidShort}-{wrapper.Name}";
                var customMenuItem = new OdinMenuItem(Tree, itemDisplayName, wrapper.DetailWrapper);
                MenuItemDict.Add(itemMenuKey, customMenuItem);
                Tree.AddMenuItemAtPath(MenuDisplayName, customMenuItem);
            }
            m_AllWrappers.AddRange(tmpRows);



        }

        public virtual bool ShowNewButton => true;


        [Button("新建行")]
        [ShowIf("@this.ShowNewButton")]
        public void NewRow()
        {
            if (Overviews == null || Overviews.Count() == 0)
            {
                return;
            }

            var newTypeWrapper = new TNewRowWrapper();
            newTypeWrapper.EnableEditOverview = true;
            newTypeWrapper.Overview = Overviews[0];
            newTypeWrapper.UnityRow = ScriptableObject.CreateInstance<TRow>();
            newTypeWrapper.UnityRow.BaseRow.Uuid = UuidUtility.GetNewUuid();
            Debug.Log($"typeof:{newTypeWrapper.GetType()}");
            m_CreateWindow = OdinEditorWindow.InspectObject(newTypeWrapper);

            newTypeWrapper.OpenWindow = m_CreateWindow;
            newTypeWrapper.AfterCreate = OnAfterCreate;
            OnNewRowPostprocess(newTypeWrapper);
        }
        public virtual void OnNewRowPostprocess(TNewRowWrapper newRowWrapper)
        {
        }

        [Button("刷新行")]
        public void RefreshRows()
        {
            foreach (var overview in Overviews)
            {
                overview.RefreshRows();
            }
        }


        void OnAfterCreate(string uuid)
        {
            InitWrappers();
            if (m_CreateWindow != null)
            {
                m_CreateWindow.Close();
            }


        }

    }
}
#endif