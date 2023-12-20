using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ClassGenerator;

namespace MetaTable
{
    public class CSharpCodeRowWriter : CsharpCodeWriterBase
    {
        List<MetaTableColumn> m_Columns;


        string m_InterfaceName;

        public CSharpCodeRowWriter(List<string> headers, List<MetaTableColumn> columns, string interfaceName)
        {
            m_Columns = columns;
            m_Headers = headers;
            m_InterfaceName = interfaceName;
        }

        public override void WriteMainClassStart(IJsonClassGeneratorConfig config, TextWriter sw)
        {
            sw.WriteLine();
            sw.WriteLine("namespace {0}", config.Namespace);
            sw.WriteLine("{");
            sw.WriteLine("    [Serializable]");

            sw.WriteLine("    {0} partial class {1} : {2},{3}", "public", JsonClassGenerator.ToTitleCase(config.MainClass), config.BaseClass, m_InterfaceName);
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

                if (config.ExamplesInDocumentation)
                {
                    sw.WriteLine(prefix + "/// <summary>");
                    sw.WriteLine(prefix + "/// Examples: " + field.GetExamplesText());
                    sw.WriteLine(prefix + "/// </summary>");
                }


                sw.WriteLine(prefix + "[JsonMember(\"{0}\")]", field.MemberName);

                var (col_index, col) = GetColumnByName(field.MemberName);
                if (col != null)
                {
                    sw.WriteLine(prefix + $"[MetaTableRowColumn(\"{field.MemberName}\",\"{col.Type}\", \"{col.CnName}\",{col_index + 1})]");
                    sw.WriteLine(prefix + "[LabelText(\"{0}\")]", col.CnName);
                }


                //使用模板Example值作为类型
                //export_path不作为类型导出
                if (config.ExamplesToType && field.Type.Type == JsonTypeEnum.String &&
                    field.JsonMemberName != "@export_path")
                    sw.WriteLine(prefix + "public {0} {1} ;", GetTypeFromExample(field.GetExamplesText()),
                        field.MemberName);
                else
                    sw.WriteLine(prefix + "public {0} {1} ;", field.Type.GetTypeName(), field.MemberName);


                sw.WriteLine();
                sw.WriteLine(prefix + $"{GetTypeFromExample(field.GetExamplesText())} {m_InterfaceName}.{field.MemberName} " + "{get => " + field.MemberName + "; set => " + $"{field.MemberName} = value" + ";}");

            }
        }
    }
}