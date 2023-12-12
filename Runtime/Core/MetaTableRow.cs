using Sirenix.OdinInspector;
using LitJson;

namespace MetaTable
{
    public abstract class MetaTableRow
    {
        // [TableTitleGroup("UUID")]
        // [HideLabel]
        [ReadOnly]
        [JsonMember("Uuid")]
        [MetaTableRowColumn("Uuid", "string", "UUID", -1)]
        public string Uuid;

        // [TableTitleGroup("Name")]
        // [HideLabel]
        [JsonMember("Name")]
        [LabelText("名字")]
        [MetaTableRowColumn("Name", "string", "名字", 0)]
        public string Name;

    }

}