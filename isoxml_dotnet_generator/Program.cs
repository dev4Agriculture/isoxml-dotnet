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
        static private void processNamespace(List<string> files, string targetNamespace) {
            var namespaceProvider = new NamespaceProvider {
                GenerateNamespace = key => targetNamespace
            };

            NamingScheme namingScheme = new NamingScheme();
            NamingProvider namingProvider = new ISOXMLNamingProvider(namingScheme);

            var generator = new Generator
            {
                OutputFolder = "./out/",
                // GenerateNullables = true,
                NamespaceProvider = namespaceProvider,
                NamingProvider = namingProvider,
            };

            generator.Generate(files);
        }

        static void Main(string[] args)
        {
            string folder = "./resources/xsd/";

            processNamespace(new List<string>() {
                folder + "ISO11783_TaskFile_V4-3.xsd",
                folder + "ISO11783_ExternalFile_V4-3.xsd",
            }, "Dev4ag.ISO11783.TaskFile");

            processNamespace(new List<string>() {
                folder + "ISO11783_LinkListFile_V4-3.xsd",
            }, "Dev4ag.ISO11783.LinkListFile");

            processNamespace(new List<string>() {
                folder + "ISO11783_TimeLog_V4-3.xsd",
            }, "Dev4ag.ISO11783.TimeLog");
        }
    }
}
