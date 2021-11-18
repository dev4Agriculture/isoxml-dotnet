using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

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
            string folder = String.Join(" ",args).Trim();
            string xsdString = File.ReadAllText(folder + "ISO11783_TaskFile_V4-3.xsd");
            TextReader textReader = new StringReader(xsdString);
            XmlSchema xmlSchema = XmlSchema.Read(textReader, ValidationCallBack);
            XmlSchemas xmlSchemas = new XmlSchemas();
            xmlSchemas.Add(xmlSchema);
            XmlSchemaImporter xmlSchemaImporter = new XmlSchemaImporter(xmlSchemas);
            /*xmlSchemaImporter.
            CodeDOM codeDom = new CodeDom();   
            File.WriteAllText(folder + "ISO11783_TaskFile.cs",formattedCode);*/
        }

    }
}
