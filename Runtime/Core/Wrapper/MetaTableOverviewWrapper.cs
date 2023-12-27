#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

using MetaTable;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;

namespace MetaTable
{
    public class MetaTableOverviewWrapper<TOverview, TDetalRowRrapper, TTableRowWrapper, TNewRowWrapper, TRow>
            where TDetalRowRrapper : MetaTableDetailRowWrapper<TOverview, TRow>, new()
            where TTableRowWrapper : MetaTableRowWrapper<TOverview, TNewRowWrapper, TRow>, new()
            where TNewRowWrapper : MetaTableNewRowWrapper<TOverview, TRow>, new()
            where TOverview : MetaTableOverview
            where TRow : MetaTableUnityRow, new()
    {
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
        [TableList(IsReadOnly = true, AlwaysExpanded = true), ShowInInspector]
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
            foreach (var overview in m_Overviews)
            {

                m_AllWrappers.AddRange(overview.UnityBaseRows.Select(x =>
                {
                    var detailWrapper = new TDetalRowRrapper();
                    detailWrapper.Overview = overview;
                    detailWrapper.UnityRow = x as TRow;
                    detailWrapper.MenuWindow = MenuWindow;

                    var wrapper = new TTableRowWrapper();
                    wrapper.Overview = overview;
                    wrapper.UnityRow = x as TRow;
                    wrapper.MenuWindow = MenuWindow;
                    wrapper.DetailWrapper = detailWrapper;
                    // wrapper.OnRemove += OnWrapperRemove;
                    return wrapper;
                }).ToList());
            }

            foreach (var wrapper in m_AllWrappers)
            {
                var itemMenuKey = wrapper.Uuid;
                // Debug.Log($"wrapper:{wrapper}");
                var itemDisplayName = $"{MenuDisplayName}-{wrapper.UuidShort}-{wrapper.Name}";
                var customMenuItem = new OdinMenuItem(Tree, itemDisplayName, wrapper.DetailWrapper);
                MenuItemDict.Add(itemMenuKey, customMenuItem);
                Tree.AddMenuItemAtPath(MenuDisplayName, customMenuItem);
            }

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
            newTypeWrapper.Overview = Overviews[0];
            newTypeWrapper.UnityRow = ScriptableObject.CreateInstance<TRow>();
            newTypeWrapper.UnityRow.BaseRow.Uuid = UuidUtility.GetNewUuid();
            Debug.Log($"typeof:{newTypeWrapper.GetType()}");
            m_CreateWindow = OdinEditorWindow.InspectObject(newTypeWrapper);

            newTypeWrapper.OpenWindow = m_CreateWindow;
            newTypeWrapper.AfterCreate = OnAfterCreate;
        }

        void OnAfterCreate(string uuid)
        {
            InitWrappers();
        }

    }
}
#endif