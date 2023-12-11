using System.Collections.Generic;
using System.Text;
using LitJson;

namespace MetaTable
{
    public static class CodeGeneratorJson
    {
#if UNITY_EDITOR
        public static string BuildRowCodeJson(List<MetaTableColumn> columns)
        {
            StringBuilder sb = new StringBuilder();
            JsonWriter writer = new JsonWriter(sb);
            writer.WriteObjectStart();

            for (int i = 0; i < columns.Count; i++)
            {
                writer.WritePropertyName(columns[i].Name);
                writer.Write(columns[i].Type);
            }

            writer.WriteObjectEnd();
            return sb.ToString();
        }
#endif
    }
}