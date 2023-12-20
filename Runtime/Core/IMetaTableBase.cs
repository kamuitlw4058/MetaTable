using Sirenix.OdinInspector;
using LitJson;
using System.Collections.Generic;

namespace MetaTable
{
    public interface IMetaTableBase
    {
        Dictionary<string, IMetaTableRow> Dict { get; set; }

        T GetRowByUuid<T>(string uuid) where T : class, IMetaTableRow;

        T GetRowById<T>(int id) where T : class, IMetaTableRow;

    }

}
