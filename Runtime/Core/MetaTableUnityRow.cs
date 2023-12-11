using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MetaTable
{
    public abstract class MetaTableUnityRow : ScriptableObject
    {

        public abstract MetaTableRow BaseRow { get; }

    }

}
