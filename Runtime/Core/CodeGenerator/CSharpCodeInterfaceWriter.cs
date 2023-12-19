using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ClassGenerator;

namespace MetaTable
{
    public class CSharpCodeInterfaceWriter : CsharpCodeWriterBase
    {
        List<MetaTableColumn> m_Columns;

        public CSharpCodeInterfaceWriter(List<string> headers, List<MetaTableColumn> columns)
        {
            m_Columns = columns;
            m_Headers = headers;
        }

        public override void WriteMainClassStart(IJsonClassGeneratorConfig config, TextWriter sw)
        {
            sw.WriteLine();
            sw.WriteLine("namespace {0}", config.Namespace);
            sw.WriteLine("{");
            sw.WriteLine("    {0} partial interface {1} : {2}", "public", JsonClassGenerator.ToTitleCase(config.MainClass), config.BaseClass);
            sw.WriteLine("    {");
        }



        public (int, MetaTableColumn) GetColumnByName(string name)
        {
            foreach (var col in m_Columns)
            {
                if (col.Name == name)
                {
                    return (m_Columns.IndexOf(col), col);
                }
            }
            return (-1, null);
        }


        public override void WriteClassMembers(IJsonClassGeneratorConfig config, TextWriter sw, JsonType type, string prefix)
        {
            foreach (var field in type.Fields)
            {

                if (config.BaseFields != null && config.BaseFields.Contains(field.MemberName))
                {
                    continue;
                }


                if (config.UsePascalCase || config.ExamplesInDocumentation)
                    sw.WriteLine();




                var (col_index, col) = GetColumnByName(field.MemberName);

                //使用模板Example值作为类型
                //export_path不作为类型导出
                if (config.ExamplesToType && field.Type.Type == JsonTypeEnum.String &&
                    field.JsonMemberName != "@export_path")
                {
                    string typeName = GetTypeFromExample(field.GetExamplesText());
                    string memberName = field.MemberName;
                    string typeAndName = $"{GetTypeFromExample(field.GetExamplesText())} {field.MemberName}";
                    sw.WriteLine(prefix + $"public {typeAndName}" + "{ get; set; }");
                }
                else
                    sw.WriteLine(prefix + "public {0} {1} {get;set;}", field.Type.GetTypeName(), field.MemberName);
            }
        }
    }
}