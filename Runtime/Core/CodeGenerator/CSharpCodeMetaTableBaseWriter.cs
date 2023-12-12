using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ClassGenerator;

namespace MetaTable
{
    public class CSharpCodeMetaTableBaseWriter : CsharpCodeWriterBase
    {

        public delegate void AdditionFunction(IJsonClassGeneratorConfig config, TextWriter sw);

        AdditionFunction m_AdditionFunction;
        public CSharpCodeMetaTableBaseWriter(List<string> headers, AdditionFunction additionFunction = null)
        {
            m_Headers = headers;
            m_AdditionFunction = additionFunction;
        }


        public override void WriteFileStart(IJsonClassGeneratorConfig config, TextWriter sw)
        {
            if (config.IsWriteFileHeader)
            {
                foreach (var line in JsonClassGenerator.FileHeader)
                {
                    sw.WriteLine("// " + line);
                }
            }

            if (config.IsTotalEditor)
            {
                sw.WriteLine($"#if UNITY_EDITOR");
            }


            sw.WriteLine();
            if (m_Headers != null)
            {
                foreach (var header in m_Headers)
                {
                    sw.WriteLine($"using {header};");
                }
            }

            if (config.IsUseUnityEditor)
            {
                if (!config.IsTotalEditor)
                {
                    sw.WriteLine($"#if UNITY_EDITOR");
                }

                sw.WriteLine($"using UnityEditor;");
                if (!config.IsTotalEditor)
                {
                    sw.WriteLine($"#endif");
                }
            }

            if (ShouldApplyNoPruneAttribute(config) || ShouldApplyNoRenamingAttribute(config))
                sw.WriteLine("using System.Reflection;");
        }

        public override void WriteFileEnd(IJsonClassGeneratorConfig config, TextWriter sw)
        {
            sw.WriteLine("    }");
            sw.WriteLine("}");
            if (config.IsTotalEditor)
            {
                sw.WriteLine($"#endif");
            }
        }


        public override void WriteMainClassStart(IJsonClassGeneratorConfig config, TextWriter sw)
        {
            sw.WriteLine();
            sw.WriteLine("namespace {0}", config.Namespace);
            sw.WriteLine("{");

            if (config.IsSerializable)
            {
                sw.WriteLine("    [Serializable]");
            }
            if (config.IsAddCreateAssetMenu)
            {
                var prefix = config.AssetMenuPrefix != null ? $"{config.AssetMenuPrefix}/" : string.Empty;
                sw.WriteLine($"        [CreateAssetMenu(fileName = \"{config.MainClass}\", menuName = \"{prefix}{config.MainClass}\")]");
            }

            if (config.BaseClass != null)
            {
                sw.WriteLine("    {0} partial class {1} : {2}", "public", JsonClassGenerator.ToTitleCase(config.MainClass), config.BaseClass);
            }
            else
            {
                sw.WriteLine("    {0} partial class {1} ", "public", JsonClassGenerator.ToTitleCase(config.MainClass));
            }
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

                if (config.ExamplesInDocumentation)
                {
                    sw.WriteLine(prefix + "/// <summary>");
                    sw.WriteLine(prefix + "/// Examples: " + field.GetExamplesText());
                    sw.WriteLine(prefix + "/// </summary>");
                }


                if (config.UseJsonMember)
                {
                    sw.WriteLine(prefix + "[JsonMember(\"{0}\")]", field.MemberName);
                }

                //使用模板Example值作为类型
                //export_path不作为类型导出
                if (config.ExamplesToType && field.Type.Type == JsonTypeEnum.String &&
                    field.JsonMemberName != "@export_path")
                    sw.WriteLine(prefix + "public {0} {1} ;", GetTypeFromExample(field.GetExamplesText()),
                        field.MemberName);
                else
                    sw.WriteLine(prefix + "public {0} {1} ;", field.Type.GetTypeName(), field.MemberName);
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