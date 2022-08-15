using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using XmlSchemaClassGenerator;

namespace Dev4Agriculture.ISO11783.ISOXML.Generation
{
    internal class ISOXMLNamingProvider : NamingProvider
    {
        public ISOXMLNamingProvider(NamingScheme scheme) : base(scheme)
        {
        }

        public override string AttributeNameFromQualifiedName(XmlQualifiedName qualifiedName, XmlSchemaAttribute xmlAttribute)
        {
            var documentations = GetDocumentation(xmlAttribute);
            if (documentations.Count() > 0)
            {
                var name = Regex.Replace(documentations[0].Text, @"\t|\n|\r|,|(\s+)", "");
                return base.AttributeNameFromQualifiedName(new XmlQualifiedName(name, qualifiedName.Namespace), xmlAttribute);
            }
            return base.AttributeNameFromQualifiedName(qualifiedName, xmlAttribute);
        }

        public override string ComplexTypeNameFromQualifiedName(XmlQualifiedName qualifiedName, XmlSchemaComplexType complexType)
        {
            var documentations = GetDocumentation(complexType.Parent as XmlSchemaAnnotated);
            if (documentations.Count() > 0)
            {
                var name = Regex.Replace(documentations[0].Text, @"\t|\n|\r|,|(\s+)", "");
                if (!name.StartsWith("ISO"))
                {
                    name = "ISO" + name;
                }
                return base.ComplexTypeNameFromQualifiedName(new XmlQualifiedName(name, qualifiedName.Namespace), complexType);
            }
            return base.ComplexTypeNameFromQualifiedName(qualifiedName, complexType);
        }

        public override string ElementNameFromQualifiedName(XmlQualifiedName qualifiedName, XmlSchemaElement xmlElement)
        {
            var documentations = GetDocumentation(xmlElement);
            if (documentations.Count() > 0)
            {
                var name = Regex.Replace(documentations[0].Text, @"\t|\n|\r|,|(\s+)", "");

                return base.ElementNameFromQualifiedName(new XmlQualifiedName(name, qualifiedName.Namespace), xmlElement);
            }
            return base.ElementNameFromQualifiedName(qualifiedName, xmlElement);
        }

        public override string EnumMemberNameFromValue(string enumName, string value, XmlSchemaEnumerationFacet xmlFacet)
        {
            var documentations = GetDocumentation(xmlFacet);
            if (documentations.Count() > 0)
            {
                var name = Regex.Replace(documentations[0].Text, @"\t|\n|\r|,|(\s+)", "");
                return base.EnumMemberNameFromValue(enumName, name, xmlFacet);
            }
            return base.EnumMemberNameFromValue(enumName, value, xmlFacet);
        }

        public override string EnumTypeNameFromQualifiedName(XmlQualifiedName qualifiedName, XmlSchemaSimpleType xmlSimpleType)
        {
            var documentations = GetDocumentation(xmlSimpleType.Parent as XmlSchemaAnnotated);
            if (documentations.Count() > 0)
            {
                var name = Regex.Replace(documentations[0].Text, @"\t|\n|\r|,|(\s+)", "");
                if (!name.StartsWith("ISO"))
                {
                    name = "ISO" + name;
                }
                return base.EnumTypeNameFromQualifiedName(new XmlQualifiedName(name, qualifiedName.Namespace), xmlSimpleType);
            }
            return base.EnumTypeNameFromQualifiedName(qualifiedName, xmlSimpleType);
        }

        private static List<DocumentationModel> GetDocumentation(XmlSchemaAnnotated annotated)
        {
            if (annotated.Annotation == null)
            {
                return new List<DocumentationModel>();
            }

            return annotated.Annotation.Items.OfType<XmlSchemaDocumentation>()
                .Where(d => d.Markup != null && d.Markup.Any())
                .Select(d => new DocumentationModel { Language = d.Language, Text = new XText(d.Markup.First().InnerText).ToString() })
                .Where(d => !string.IsNullOrEmpty(d.Text))
                .ToList();
        }
    }
}
