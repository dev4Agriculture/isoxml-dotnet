using System;
using System.CodeDom;
using XmlSchemaClassGenerator;

namespace Dev4Agriculture.ISO11783.ISOXML.Generation;

internal class ISOXMLMemberVisitor
{
    public ISOXMLMemberVisitor()
    {
    }


    public static void Visit(CodeTypeMember arg1, PropertyModel arg2)
    {
        Console.WriteLine("Arg1: " + arg1.Name + " Property Model: " + arg2.Name);
    }

}
