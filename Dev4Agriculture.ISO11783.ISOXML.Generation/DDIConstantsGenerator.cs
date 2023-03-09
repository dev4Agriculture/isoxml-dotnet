using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Dev4Agriculture.ISO11783.ISOXML.Generation;


public static class DDIConstantsGenerator
{
    public struct DDIEntry
    {
        public string DDIName;
        public ushort DDIValue;
        public string Definition;
        public string Unit;
        public float Resolution;
    };

    public static Regex rgx = new Regex("[^a-zA-Z0-9_]");
    public static TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

    private const string DDIEntryText = "\n     public class DDIEntry\n     {\n        public int Id;\n        public string Name;\n        public string Description;\n        public string Unit;\n        public float Resolution;\n     }\n";

    private const string DDIDictionaryText = "\n     public class DDIInfo\n     {\n         public static readonly Dictionary<DDIList, DDIEntry> DDICollection = new Dictionary<DDIList, DDIEntry>\n         {\n";

    public static string FormatDDIName(List<string> parts)
    {
        var ddiName = "";
        foreach( var part in parts )
        {
            if(int.TryParse(part, out _))
            {
                ddiName+= "_"+part+"_";
            } else
            {
                ddiName += part+" ";
            }
        }

        ddiName = FormatSymbols(ddiName);
        ddiName = textInfo.ToTitleCase(ddiName);
        ddiName = rgx.Replace(ddiName, " ");
        ddiName = ddiName.Replace(" ", "");
        if (ddiName.EndsWith("_"))
        {
            ddiName = ddiName.Substring(0, ddiName.Length - 3);
        }
        return ddiName;

    }

    //Yes, we could use the SourceGenerator. This way it feels just a bit more convenient to me :)
    public static void Generate(string source)
    {
        var lines = File.ReadAllText(source).Split("\n").Select(entry => entry.Replace("\r",""));
        var entities = new List<DDIEntry>();
        var curEntity = new DDIEntry();

        foreach (var line in lines)
        {
            if (line.Contains(':'))
            {
                var splitted = line.Split(':').ToList();
                var command = splitted[0];
                splitted.RemoveAt(0);
                var arguments = string.Join(":", splitted);
                switch (command)
                {
                    case "DD Entity":
                        if (!string.IsNullOrEmpty(curEntity.DDIName))
                        {
                            entities.Add(curEntity);
                            curEntity = new DDIEntry();
                        }
                        var ddiNameList = arguments.Split(" ").ToList();
                        ddiNameList = ddiNameList.Where(x => !string.IsNullOrEmpty(x.Trim())).ToList();
                        curEntity.DDIValue = ushort.Parse(ddiNameList.First());
                        ddiNameList.RemoveAt(0);
                        curEntity.DDIName = FormatDDIName(ddiNameList);

                        break;
                    //data example: Maximum Application Rate specified as distance: e.g. seed spacing of a precision seeder
                    case "Definition":
                        curEntity.Definition = arguments.Replace("\"", "\\\"");
                        break;
                    //data example: mm³/m² - Capacity per area unit
                    case "Unit":
                        var unitData = arguments.Split(" - ").FirstOrDefault();
                        curEntity.Unit = FormatSymbols(unitData.Trim());
                        break;
                    //data example:  1
                    case "Resolution":
                        if (float.TryParse(arguments.Trim(), out var result))
                            curEntity.Resolution = result;
                        break;
                    default:
                        break;

                }
            }
        }

        var enText = "using System.Collections.Generic; \n\n" +
                     "namespace de.dev4Agriculture.ISOXML.DDI{\n" +
                     "    public enum DDIList\n{\n";
        foreach (var entry in entities)
        {
            enText += "        " + entry.DDIName + " = 0x" + entry.DDIValue.ToString("X4") + ",\n";
        }
        enText += "    }\n" + DDIEntryText;
        enText += DDIDictionaryText;
        foreach (var entity in entities)
        {
            enText += $"\n             {{DDIList.{entity.DDIName}, new DDIEntry{{Id = {entity.DDIValue}, Name = \"{entity.DDIName}\", Description = \"{entity.Definition}\"" +
                      $", Resolution = {entity.Resolution}, Unit = \"{entity.Unit}\"}} }},";
        }
        enText += "\n         };\n     }\n}\n";
        File.WriteAllText("../../../../Dev4Agriculture.ISO11783.ISOXML/_generated/DDIList.cs", enText);
    }

    private static string FormatSymbols(string text)
    {
        text = text.Replace("__", "_");
        text = text.Replace("-", "_");
        text = text.Replace("²", "_2").Replace("³", "_3");
        return text;
    }

}
