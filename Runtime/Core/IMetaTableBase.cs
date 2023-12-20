using Sirenix.OdinInspector;
using LitJson;
using System.Collections.Generic;

namespace MetaTable
{
    public interface IMetaTableBase
    {
        Dictionary<string, IMetaTableRow> Dict { get; set; }

        IMetaTableRow GetRowByUuid(string uuid);

        IMetaTableRow GetRowById(int id);

    }

}
