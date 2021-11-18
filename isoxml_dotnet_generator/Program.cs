using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using XmlSchemaClassGenerator;

namespace isoxml_dotnet_generator
{


    class Program
    {
        private static string curClass = "";

        static void onVisitMember(CodeTypeMember codeTypeMember, PropertyModel propertyModel)
        {
            Console.WriteLine(" Param: " + codeTypeMember.Name);
            if (propertyModel.Documentation.Count > 0)
            {
                var name  = Regex.Replace(propertyModel.Documentation[0].Text, @"\t|\n|\r", "");
                if(name.IndexOf(" ") != -1)
                {
                    Console.WriteLine("Skipped Attribute " + name);
                    return;
                }
                if(codeTypeMember.Name.IndexOf("Specified") != -1)
                {
                    codeTypeMember.Name = curClass + name + "Specified";
                    propertyModel.Name = name;
                    propertyModel.Type.Name = name;
                }else
                {
                    codeTypeMember.Name = curClass+name;
                    propertyModel.Name = name;
                    propertyModel.Type.Name = name;

                }
            }
        }

        private static void onType(CodeTypeDeclaration codeType, TypeModel typeModel)
        {
            Console.WriteLine("Class: " + codeType.Name);
            if (typeModel.Documentation.Count > 0)
            {
                var name = Regex.Replace(typeModel.Documentation[0].Text, @"\t|\n|\r", "");
                if (name.IndexOf(" ") != -1)
                {
                    Console.WriteLine("Skipped Class " + name);
                    return;
                }
                curClass = codeType.Name;
                codeType.Name = name;
            }
        }


        static void Main(string[] args)
        {
            string folder = "./resources/xsd/";
            
            List<String> files = new List<string>() { folder + "ISO11783_TaskFile_V4-3.xsd" };
            var generator = new Generator
            {
                OutputFolder = "./out/",
                Log = s => Console.Out.WriteLine(s),
                GenerateNullables = true,
                NamespaceProvider = new Dictionary<NamespaceKey, string>
    {
        { new NamespaceKey("http://dev4Agriculture.de"), "de.dev4ag.iso11783" }
    }
                .ToNamespaceProvider(new GeneratorConfiguration { NamespacePrefix = "de.dev4ag.iso11783" }.NamespaceProvider.GenerateNamespace)
            };

            Dictionary<String, String> replacementDict = new Dictionary<string, string>();
            replacementDict.Add("PTN", "Position");
            replacementDict.Add("ASP", "AllocationStamp");
            replacementDict.Add("TIM", "Time");

            NamingScheme namingScheme = new NamingScheme();
            NamingProvider namingProvider = new ISOXMLNamingProvider(namingScheme, replacementDict);
            generator.GenerateNullables = false;
            generator.MemberVisitor = onVisitMember;
            generator.TypeVisitor = onType;
            generator.NamingProvider = namingProvider;
            generator.Generate(files);

        }


    }
}
