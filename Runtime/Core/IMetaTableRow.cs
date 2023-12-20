using Sirenix.OdinInspector;
using LitJson;

namespace MetaTable
{
    public interface IMetaTableRow
    {
        public string Uuid { get; set; }

        public string Name { get; set; }

        public int Id { get; set; }

        public string UuidShort { get; }

    }

}
