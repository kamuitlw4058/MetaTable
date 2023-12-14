using System.Linq;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace MetaTable.Editor
{
    public class MetaTableEditor : OdinMenuEditorWindow
    {
        [MenuItem("MetaTable/表格编辑器", false, 10)]
        private static void OpenWindow()
        {
            var window = GetWindow<MetaTableEditor>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(1100, 700);
            window.titleContent = new GUIContent("表格编辑器");
            window.MenuWidth = 250;
        }
        protected override void OnBeginDrawEditors()
        {
            if (MenuTree == null)
                return;

            var toolbarHeight = MenuTree.Config.SearchToolbarHeight;

            SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight);
            {
                // GUILayout.Label("提交拉取前务必点击保存全部配置");


                if (SirenixEditorGUI.ToolbarButton(new GUIContent("刷新菜单树")))
                {
                    ForceMenuTreeRebuild();
                }

            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }

        void InitOverviews<TOverview, TDetailRowWrapper, TTableRowWrapper, TNewRowWrapper, TRow>(OdinMenuTree tree, string menuMainKey, string menuDisplayName)
            where TOverview : MetaTableOverview
            where TDetailRowWrapper : MetaTableDetailRowWrapper<TOverview, TRow>, new()
            where TTableRowWrapper : MetaTableRowWrapper<TOverview, TNewRowWrapper, TRow>, new()
            where TNewRowWrapper : MetaTableNewRowWrapper<TOverview, TRow>, new()
            where TRow : MetaTableUnityRow, new()
        {
            var overviews = AssetDatabaseUtility.FindAsset<TOverview>().ToList();
            // var overviewEditor = new OverviewEditorBase<TOverview, TRowDetailWrapper, TTableRowWrapper, TNewRowWrapper, TRow>();
            var overviewEditor = new MetaTableOverviewWrapper<TOverview,
                                   TDetailRowWrapper,
                                  TTableRowWrapper,
                                   TNewRowWrapper,
                                    TRow>();

            overviewEditor.Overviews = overviews;
            overviewEditor.MenuWindow = this;
            overviewEditor.MenuDisplayName = menuDisplayName;
            overviewEditor.Tree = tree;
            overviewEditor.InitWrappers();
            tree.Add(menuDisplayName, overviewEditor);
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(false);
            tree.Config.DrawSearchToolbar = true;
            tree.Config.AutoScrollOnSelectionChanged = false;

            // InitOverviews<AssetGroupOverview,
            //     AssetGroupDetailRowWrapper,
            //    AssetGroupRowWrapper,
            //    AssetGroupNewRowWrapper,
            //     UnityAssetGroupRow>(tree, "AssetGroup", "资源组");

            return tree;
        }
    }

}