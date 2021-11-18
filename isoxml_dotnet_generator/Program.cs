using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using XmlSchemaClassGenerator;

namespace isoxml_dotnet_generator
{
    class Program
    {

        private static void ValidationCallBack(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
                Console.WriteLine("\tWarning: Matching schema not found.  No validation occurred." + args.Message);
            else
                Console.WriteLine("\tValidation error: " + args.Message);
        }

        static void Main(string[] args)
        {
            string folder = "./resources/xsd/";
            /*string xsdString = File.ReadAllText(folder + "ISO11783_TaskFile_V4-3.xsd");
            TextReader textReader = new StringReader(xsdString);
            XmlSchema xmlSchema = XmlSchema.Read(textReader, ValidationCallBack);
            XmlSchemas xmlSchemas = new XmlSchemas();
            xmlSchemas.Add(xmlSchema);
            XmlSchemaImporter xmlSchemaImporter = new XmlSchemaImporter(xmlSchemas);

            XmlSchemaSet xmlSchemaSet = XmlSchemaSet.
            */
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
            NamingScheme namingScheme = new NamingScheme();
            NamingProvider namingProvider = new ISOXMLNamingProvider(namingScheme);

            
            generator.NamingProvider = namingProvider;
            generator.Generate(files);

        }

    }
}
