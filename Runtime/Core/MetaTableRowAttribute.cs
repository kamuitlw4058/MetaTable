using System;

namespace MetaTable
{
    public class MetaTableRowColumnAttribute : Attribute
    {
        public string Name { get; }

        public string ColType { get; }

        public string NameCn { get; }

        public int Index { get; }

        public MetaTableRowColumnAttribute(string name, string col_type, string name_cn, int index)
        {
            Name = name;
            ColType = col_type;
            NameCn = name_cn;
            Index = index;
        }
    }
}