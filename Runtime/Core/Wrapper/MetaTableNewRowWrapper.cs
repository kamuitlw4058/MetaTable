#if UNITY_EDITOR
using Sirenix.OdinInspector;
using System.Collections;
using System.Linq;
using UnityEngine;
using System;

using UnityEditor;
using Sirenix.OdinInspector.Editor;


namespace MetaTable
{
    public class MetaTableNewRowWrapper<TOverview, TRow> : MetaTableWrapperBase<TOverview, TRow>
                        where TOverview : MetaTableOverview
                        where TRow : MetaTableUnityRow, new()
    {

        public override bool CanNameChange => true;
        public Action<string> AfterCreate;

        protected override bool CheckExistsUuid()
        {
            return OverviewUtility.ExistsOverviewUuid<TOverview>(Uuid);
        }


        public override string Name
        {
            get
            {

                return UnityRow.BaseRow.Name;
            }
            set
            {

                UnityRow.BaseRow.Name = value;
            }
        }


        protected override bool CheckExistsName()
        {
            return OverviewUtility.ExistsOverviewName<TOverview>(Name);
        }


        [Button("新建")]
        // [ShowIf("@this.ShowCreateButton")]
        public virtual void Create()
        {
            if (UnityRow == null) return;

            if (CheckExistsUuid() || CheckExistsName())
            {
                Debug.Log($"Row: id:{Uuid}  exists:{CheckExistsUuid()} name:{Name} exists:{CheckExistsName()}");
                return;
            }

            Overview.AddRow(UnityRow);

            if (AfterCreate != null)
            {
                AfterCreate(Uuid);
            }
        }


    }
}

#endif