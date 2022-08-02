using Microsoft.CodeAnalysis;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using XmlSchemaClassGenerator;

namespace isoxml_dotnet_generator
{


    class Program
    {
        static string outFolder = "./out/";
        static string destinationFolder = "../../../../isoxml_dotnet_core/src/xml/";

        static private void processNamespace(List<string> files, string targetNamespace) {
            var namespaceProvider = new NamespaceProvider {
                GenerateNamespace = key => targetNamespace
            };

            NamingScheme namingScheme = new NamingScheme();
            NamingProvider namingProvider = new ISOXMLNamingProvider(namingScheme);

            var generator = new Generator
            {
                OutputFolder = outFolder,
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

            //Copy updated classes to main project
            string[] files = Directory.GetFiles(outFolder,"*.cs");
            foreach(string file in files)
            {
                File.Copy(file, destinationFolder + Path.GetFileName(file));
            }

        }
    }
}
