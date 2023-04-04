using System.Collections.Generic;
using System.IO;
using XmlSchemaClassGenerator;


namespace Dev4Agriculture.ISO11783.ISOXML.Generation;

internal class Program
{
    //TODO It's more save to move this Element to a common class. It's found in Generator and DotnetCore Library
    private const string ISOXMLClassName = "Dev4Agriculture.ISO11783.ISOXML";
    private const string OutFolder = "./out/";
    private const string DestinationFolder = "../../../../Dev4Agriculture.ISO11783.ISOXML/_generated/";

    private static void ProcessNamespace(List<string> files, string targetNamespace)
    {
        var namespaceProvider = new NamespaceProvider
        {
            GenerateNamespace = key => targetNamespace
        };

        var namingScheme = new NamingScheme();
        NamingProvider namingProvider = new ISOXMLNamingProvider(namingScheme);

        var generator = new Generator()
        {
            OutputFolder = OutFolder,
            NamespaceProvider = namespaceProvider,
            NamingProvider = namingProvider,
            MemberVisitor = ISOXMLMemberVisitor.Visit,
            AssemblyVisible = false,
            GenerateNullables = true
        };

        generator.Generate(files);
    }


    public static void GenerateFromXSDs(string[] args)
    {
        var folder = "./resources/xsd/";

        ProcessNamespace(new List<string>() {
            folder + "ISO11783_TaskFile_V4-3.xsd",
            folder + "ISO11783_ExternalFile_V4-3.xsd",
        }, ISOXMLClassName + ".TaskFile");

        ProcessNamespace(new List<string>() {
            folder + "ISO11783_LinkListFile_V4-3.xsd",
        }, ISOXMLClassName + ".LinkListFile");


        //Copy updated classes to main project
        var files = Directory.GetFiles(OutFolder, "*.cs");
        if (Directory.Exists(DestinationFolder))
        {
            Directory.Delete(DestinationFolder, true);
        }
        Directory.CreateDirectory(DestinationFolder);

        foreach (var file in files)
        {
            File.Copy(file, DestinationFolder + Path.GetFileName(file));
        }

    }

    public static void GenerateFromDDIList(string[] args)
    {
        DDIConstantsGenerator.Generate("./resources/txt/ddiExport.txt");
    }


    public static void Main(string[] args)
    {
        if (args is null)
        {
            throw new System.ArgumentNullException(nameof(args));
        }


        GenerateFromXSDs(args);
        GenerateFromDDIList(args);


    }
}
