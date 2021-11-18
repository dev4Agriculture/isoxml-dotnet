using Microsoft.CodeAnalysis;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using XmlSchemaClassGenerator;

namespace isoxml_dotnet_generator
{


    class Program
    {
        // private static string curClass = "";

        static Dictionary<string, string> fullNames = new Dictionary<string, string>();
        // static void memberCollector(CodeTypeMember codeTypeMember, PropertyModel propertyModel) {
        //     if (propertyModel.Documentation.Count > 0) {
        //         var name = Regex.Replace(propertyModel.Documentation[0].Text, @"\t|\n|\r|,|(\s+)", "");
        //         fullNames[propertyModel.Name] = name;
        //     }
        // }
        static void typeCollector(CodeTypeDeclaration codeType, TypeModel typeModel) {
            if (typeModel.Documentation.Count > 0) {
                var name = Regex.Replace(typeModel.Documentation[0].Text, @"\t|\n|\r|,|(\s+)", "");
                var originalTag = typeModel.Name.Replace("_", "");
                fullNames[originalTag] = name;
                fullNames[typeModel.Name] = name;
            }
        }

        static void onVisitMember(CodeTypeMember codeTypeMember, PropertyModel propertyModel)
        {
            // Console.WriteLine(" Param: " + codeTypeMember.Name);
            if (propertyModel.Documentation.Count > 0)
            {
                var name = Regex.Replace(propertyModel.Documentation[0].Text, @"\t|\n|\r|,|(\s+)", "");
                if(codeTypeMember.Name.IndexOf("Specified") != -1)
                {
                    propertyModel.Name = $"{name}Specified";
                }
                else
                {
                    Console.WriteLine($"{propertyModel.Name} {name}");
                    propertyModel.Name = name;
                }
            }
        }

        private static void onType(CodeTypeDeclaration codeType, TypeModel typeModel)
        {
            // Console.WriteLine("Class: " + codeType.Name);
            if (typeModel.Documentation.Count > 0)
            {
                var name = Regex.Replace(typeModel.Documentation[0].Text, @"\t|\n|\r|,|(\s+)", "");
                codeType.Name = name;
            }
        }


        static void Main(string[] args)
        {
            string folder = "./resources/xsd/";
            
            var files = new List<string>() { folder + "ISO11783_TaskFile_V4-3.xsd" };
            var namespaceProvider = new Dictionary<NamespaceKey, string>
                {
                    { new NamespaceKey("http://dev4Agriculture.de"), "de.dev4ag.iso11783" }
                }
                .ToNamespaceProvider(new GeneratorConfiguration { NamespacePrefix = "de.dev4ag.iso11783" }.NamespaceProvider.GenerateNamespace);


            var generatorCollector = new Generator
            {
                OutputFolder = "./out/",
                Log = s => Console.Out.WriteLine(s),
                GenerateNullables = true,
                NamespaceProvider = namespaceProvider,
                TypeVisitor = typeCollector
            };

            generatorCollector.Generate(files);

            // print names
            var lines = fullNames.Select(kvp => kvp.Key + ": " + kvp.Value.ToString());
            Console.WriteLine(string.Join(Environment.NewLine, lines));

            NamingScheme namingScheme = new NamingScheme();
            NamingProvider namingProvider = new ISOXMLNamingProvider(namingScheme, fullNames);

            var generator = new Generator
            {
                OutputFolder = "./out/",
                Log = s => Console.Out.WriteLine(s),
                GenerateNullables = false,
                NamespaceProvider = namespaceProvider,
                MemberVisitor = onVisitMember,
                TypeVisitor = onType,
                NamingProvider = namingProvider
            };

            generator.Generate(files);

        }
    }
}
