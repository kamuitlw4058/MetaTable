using Sirenix.OdinInspector;
using LitJson;
using System.Collections.Generic;

namespace MetaTable
{
    public interface IMetaTableBase
    {

        IMetaTableRow GetMetaTableRowByUuid(string uuid);

        IMetaTableRow GetMetaTableRowById(int id);

    }

}
