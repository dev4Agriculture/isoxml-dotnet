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
    };

    public static Regex rgx = new Regex("[^a-zA-Z0-9_]");
    public static TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

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
        ddiName = ddiName.Replace("__", "_");
        ddiName = ddiName.Replace("-", "_");
        ddiName = ddiName.Replace("²", "_2").Replace("³", "_3");
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
                    default:
                        break;

                }
            }
        }

        var enText = "namespace de.dev4Agriculture.ISOXML.DDI{\n" +
                     "    public enum DDIList\n{\n";
        foreach (var entry in entities)
        {
            enText += "        " + entry.DDIName + " = 0x" + entry.DDIValue.ToString("X4") + ",\n";
        }



        enText += "    }\n}\n";
        File.WriteAllText("../../../../Dev4Agriculture.ISO11783.ISOXML/_generated/DDIList.cs", enText);
    }

}
