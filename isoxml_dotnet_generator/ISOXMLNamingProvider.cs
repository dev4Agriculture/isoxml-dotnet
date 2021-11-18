using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using XmlSchemaClassGenerator;

namespace isoxml_dotnet_generator
{
    class ISOXMLNamingProvider : NamingProvider
    {
        public ISOXMLNamingProvider(NamingScheme scheme): base(scheme)
        {
            Console.WriteLine("Constructed");
        }

        public override string AttributeGroupTypeNameFromQualifiedName(XmlQualifiedName qualifiedName)
        {
            return base.AttributeGroupTypeNameFromQualifiedName(qualifiedName);
        }

        public override string AttributeNameFromQualifiedName(XmlQualifiedName qualifiedName)
        {
            return base.AttributeNameFromQualifiedName(qualifiedName);
        }

        public override string ComplexTypeNameFromQualifiedName(XmlQualifiedName qualifiedName)
        {
            return base.ComplexTypeNameFromQualifiedName(qualifiedName);
        }

        public override string ElementNameFromQualifiedName(XmlQualifiedName qualifiedName)
        {
            return base.ElementNameFromQualifiedName(qualifiedName);
        }

        public override string EnumMemberNameFromValue(string enumName, string value)
        {
            return base.EnumMemberNameFromValue(enumName, value);
        }

        public override string EnumTypeNameFromQualifiedName(XmlQualifiedName qualifiedName)
        {
            return base.EnumTypeNameFromQualifiedName(qualifiedName);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string GroupTypeNameFromQualifiedName(XmlQualifiedName qualifiedName)
        {
            return base.GroupTypeNameFromQualifiedName(qualifiedName);
        }

        public override string PropertyNameFromAttribute(string typeModelName, string attributeName)
        {
            return base.PropertyNameFromAttribute(typeModelName, attributeName);
        }

        public override string PropertyNameFromElement(string typeModelName, string elementName)
        {
            return base.PropertyNameFromElement(typeModelName, elementName);
        }

        public override string RootClassNameFromQualifiedName(XmlQualifiedName qualifiedName)
        {
            return base.RootClassNameFromQualifiedName(qualifiedName);
        }

        public override string SimpleTypeNameFromQualifiedName(XmlQualifiedName qualifiedName)
        {
            return base.SimpleTypeNameFromQualifiedName(qualifiedName);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        protected override string QualifiedNameToTitleCase(XmlQualifiedName qualifiedName)
        {
            return base.QualifiedNameToTitleCase(qualifiedName);
        }
    }
}
