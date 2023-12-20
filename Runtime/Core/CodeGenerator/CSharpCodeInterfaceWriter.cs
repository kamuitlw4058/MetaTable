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
        public delegate void AdditionFunction(IJsonClassGeneratorConfig config, TextWriter sw);


        AdditionFunction m_AdditionFunction;

        public CSharpCodeInterfaceWriter(List<string> headers, AdditionFunction additionFunction = null)
        {
            m_Headers = headers;
            m_AdditionFunction = additionFunction;
        }

        public override void WriteMainClassStart(IJsonClassGeneratorConfig config, TextWriter sw)
        {
            sw.WriteLine();
            sw.WriteLine("namespace {0}", config.Namespace);
            sw.WriteLine("{");
            sw.WriteLine("    {0} partial interface {1} : {2}", "public", JsonClassGenerator.ToTitleCase(config.MainClass), config.BaseClass);
            sw.WriteLine("    {");
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

        public override void WriteAdditionFunction(IJsonClassGeneratorConfig config, TextWriter sw)
        {

            if (m_AdditionFunction != null)
            {
                m_AdditionFunction.Invoke(config, sw);
            }

        }
    }
}