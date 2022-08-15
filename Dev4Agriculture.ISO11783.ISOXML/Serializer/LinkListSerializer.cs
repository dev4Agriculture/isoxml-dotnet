using Dev4Agriculture.ISO11783.ISOXML.LinkListFile;
using Dev4Agriculture.ISO11783.ISOXML.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace Dev4Agriculture.ISO11783.ISOXML.Serializer
{
    public class LinkListSerializer
    {

        private delegate object ValueConvertor(string value);
        private static readonly Dictionary<string, ValueConvertor> Convertors = new Dictionary<string, ValueConvertor>() {
            {"String", value => value},
            {"UInt16", value => Convert.ToUInt16(value)},
            {"UInt64", value => Convert.ToUInt64(value)},
            {"Byte", value => Convert.ToByte(value)},
            {"Int64", value => Convert.ToInt64(value)},
            {"Decimal", value => Convert.ToDecimal(value)},
            {"Double", value => Convert.ToDouble(value)},
            {"Byte[]", value => {
                if (value.Length % 2 != 0)
                {
                    throw new Exception($"value '{value}' must have even number of symbols");
                }

                try {
                    var data = new byte[value.Length / 2];
                    for (var index = 0; index < data.Length; index++)
                    {
                        var byteValue = value.Substring(index * 2, 2);
                        data[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    }
                    return data;
                } catch (Exception) {
                    throw new Exception($"Can not parse value '{value}'");
                }
            }}
        };

        private readonly Assembly _linkListAssembly;

        public List<ResultMessage> Messages { get; private set; }

        public LinkListSerializer()
        {
            Messages = new List<ResultMessage>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            _linkListAssembly = assemblies.FirstOrDefault(assembly => assembly.GetName().Name == "Dev4Agriculture.ISO11783.ISOXML");

        }
        public object Deserialize(XmlDocument xml)
        {
            Messages.Clear();
            var keepCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            object result = null;
            var index = -1;
            while (result == null && index < xml.ChildNodes.Count)
            {
                index++;
                result = ParseNode(xml.ChildNodes[index], $"{xml.ChildNodes[index].Name}[0]");
            }
            Thread.CurrentThread.CurrentCulture = keepCulture;
            return result;
        }


        public void Serialize(ISO11783LinkListFile taskData, string path)
        {
            var ser = new XmlSerializer(typeof(ISO11783LinkListFile));
            TextWriter writer = new StreamWriter(path);
            ser.Serialize(writer, taskData);
            writer.Close();
        }
        // mainly for debugging
        public HashSet<string> GetAllAttrTypes()
        {
            return _linkListAssembly.GetTypes()
                .Where(type => type.Namespace == Constants.ISOXMLClassName + ".LinkList")
                .SelectMany(type => type.GetProperties()
                    .Where(property => property.CustomAttributes.FirstOrDefault(
                        attr => attr.AttributeType.FullName == "System.Xml.Serialization.XmlAttributeAttribute"
                    ) != null && !property.PropertyType.IsEnum)
                    .Select(property => property.PropertyType.Name)
                ).Aggregate(new HashSet<string>(), (res, typeName) =>
                {
                    res.Add(typeName);
                    return res;
                });
        }

        private Type FindType(string name)
        {
            foreach (var type in _linkListAssembly.GetTypes())
            {
                foreach (var attr in type.CustomAttributes)
                {
                    if (attr.AttributeType.FullName == "System.Xml.Serialization.XmlTypeAttribute" &&
                        (string)attr.ConstructorArguments[0].Value == name
                    )
                    {
                        return type;
                    }
                }
            }
            return null;
        }

        private PropertyInfo GetPropertyByAttrName(Type type, string xmlAttrName)
        {
            foreach (var property in type.GetProperties())
            {
                foreach (var attr in property.CustomAttributes)
                {
                    if (attr.AttributeType.FullName == "System.Xml.Serialization.XmlAttributeAttribute" &&
                        (string)attr.ConstructorArguments[0].Value == xmlAttrName
                    )
                    {
                        return property;
                    }
                }
            }
            return null;
        }

        private PropertyInfo GetPropertyByElementName(Type type, string xmlElementName)
        {
            foreach (var property in type.GetProperties())
            {
                foreach (var attr in property.CustomAttributes)
                {
                    if (attr.AttributeType.FullName == "System.Xml.Serialization.XmlElementAttribute" &&
                        (string)attr.ConstructorArguments[0].Value == xmlElementName
                    )
                    {
                        return property;
                    }
                }
            }
            return null;
        }

        private string GetEnumValue(Type enumType, string xmlValue)
        {
            var enumValueMembers = enumType.GetMembers(
                BindingFlags.Public | BindingFlags.Static
            );

            foreach (var value in enumValueMembers)
            {
                foreach (var enumValueAttr in value.CustomAttributes)
                {
                    if (enumValueAttr.AttributeType.FullName == "System.Xml.Serialization.XmlEnumAttribute" &&
                        (string)enumValueAttr.ConstructorArguments[0].Value == xmlValue
                    )
                    {
                        return value.Name;
                    }
                }
            }

            return null;
        }

        // if the property is a collection, we add the value to it, otherwise, we just set the value
        private void SetValue(Type type, PropertyInfo property, object obj, object value)
        {
            if (type is null)
            {
                //TODO Proper error handling
                throw new ArgumentNullException(nameof(type));
            }

            foreach (var implInterface in property.PropertyType.GetInterfaces())
            {
                if (implInterface.Name == "IList")
                {
                    var list = (IList)property.GetValue(obj);
                    list.Add(value);
                    return;
                }
            }
            property.SetValue(obj, value);
        }

        private void AddMessage(ResultMessageType type, string message)
        {
            Messages.Add(new ResultMessage(type, message));
        }

        private void ValidateProperty(PropertyInfo property, object value, string attrValue, string linkListNodeId)
        {
            var rangeAttr = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.RangeAttribute>();
            var maxLengthAttr = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.MaxLengthAttribute>();
            var minLengthAttr = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.MinLengthAttribute>();
            var regexAttr = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.RegularExpressionAttribute>();
            if (rangeAttr != null && !rangeAttr.IsValid(value))
            {
                AddMessage(
                    ResultMessageType.Warning,
                    $"The field {property.Name} must be between {rangeAttr.Minimum} and {rangeAttr.Maximum} (path: {linkListNodeId}; value: {value})"
                );
            }

            if (maxLengthAttr != null && !maxLengthAttr.IsValid(value))
            {
                AddMessage(
                    ResultMessageType.Warning,
                    $"The field {property.Name} has length more than {maxLengthAttr.Length} (path: {linkListNodeId}; value: {attrValue})"
                );
            }

            if (minLengthAttr != null && !minLengthAttr.IsValid(value))
            {
                AddMessage(
                    ResultMessageType.Warning,
                    $"The field {property.Name} has length less than {minLengthAttr.Length} (path: {linkListNodeId}; value: {attrValue})"
                );
            }

            if (regexAttr != null && !regexAttr.IsValid(attrValue))
            {
                AddMessage(
                    ResultMessageType.Warning,
                    $"The field {property.Name} doesn't match regular expression {regexAttr.Pattern} (path: {linkListNodeId}; value: {attrValue})"
                );
            }
        }

        private void CheckRequiredProperties(Type type, object obj, string linkListNodeId)
        {
            foreach (var property in type.GetProperties())
            {
                var required = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.RequiredAttribute>() != null;

                if (required && property.GetValue(obj) == null)
                {
                    AddMessage(
                        ResultMessageType.Warning,
                        $"Missing required property {property.Name} (path: {linkListNodeId})"
                    );
                }
            }
        }

        private object CheckXMLDeclaration(XmlNode node)
        {
            if (node is null)
            {
                throw new ArgumentNullException(nameof(node));
            }
            //TODO Fill CheckXMLDeclaration
            return null;
        }


        private object ParseCDATA(XmlNode node)
        {
            AddMessage(ResultMessageType.Error, "LinkList includes CDATA-Element which is not allowed: " + node.OuterXml);
            return null;
        }

        private object ParseEntity(XmlNode node)
        {
            AddMessage(ResultMessageType.Error, "LinkList includes Entity-Element which is not allowed: " + node.OuterXml);
            return null;
        }

        private object ParseComment(XmlNode node)
        {
            AddMessage(ResultMessageType.Warning, "Comment found: " + node.InnerText);
            return null;
        }


        private object ParseText(XmlNode node)
        {
            AddMessage(ResultMessageType.Error, "LinkList includes Text-Element which is not allowed: " + node.InnerText);
            return null;
        }

        private object ParseElement(XmlNode node, string linkListNodeId = null)
        {
            var type = FindType(node.Name);
            if (type == null)
            {
                var isRoot = string.IsNullOrEmpty(linkListNodeId);
                AddMessage(
                    isRoot ? ResultMessageType.Error : ResultMessageType.Warning,
                    $"Unknown XML element {node.Name} (path: {linkListNodeId})"
                );
                return null;
            }
            var obj = Activator.CreateInstance(type);
            foreach (XmlAttribute attr in node.Attributes)
            {
                var property = GetPropertyByAttrName(type, attr.Name);

                if (property == null)
                {
                    //TODO there should be a more generic way for this.
                    //Ignore XSD Schemata information
                    if (!attr.Name.Equals("xmlns:xsi") && !attr.Name.Equals("xmlns:xsd"))
                    {

                        AddMessage(
                            ResultMessageType.Warning,
                            $"Unknown XML attribute {attr.Name} (path: {linkListNodeId})"
                        );
                    }
                    continue;
                }

                if (property.PropertyType.IsEnum)
                {
                    var enumValue = GetEnumValue(property.PropertyType, attr.Value);

                    if (enumValue == null)
                    {
                        AddMessage(
                            ResultMessageType.Warning,
                            $"Unknown enum value {attr.Value} (path: {linkListNodeId}; property: {property.Name})"
                        );
                        continue;
                    }

                    property.SetValue(obj, Enum.Parse(property.PropertyType, enumValue));
                }
                else
                {
                    Convertors.TryGetValue(property.PropertyType.Name, out var convertor);
                    try
                    {
                        var convertedAttr = convertor(attr.Value);
                        property.SetValue(obj, convertedAttr);
                        ValidateProperty(property, convertedAttr, attr.Value, linkListNodeId);
                    }
                    catch (Exception e)
                    {
                        AddMessage(
                            ResultMessageType.Warning,
                            $"Cannot parse value {attr.Value} (path: {linkListNodeId}; property: {property.Name}), Error: {e.GetType()} Message:{e.Message}"
                        );
                    }
                }
            }

            var childrenCount = new Dictionary<string, int>();

            foreach (XmlNode childNode in node.ChildNodes)
            {

                var name = childNode.Name;
                if (childrenCount.TryGetValue(name, out var count))
                {
                    childrenCount[name] = count + 1;
                }
                else
                {
                    count = 0;
                    childrenCount.Add(name, count + 1);
                }
                var childNodeLinkListId = $"{linkListNodeId}->{name}[{count}]";

                var parsedNode = ParseNode(childNode, childNodeLinkListId);

                if (parsedNode != null)
                {
                    var property = GetPropertyByElementName(type, name);
                    if (property == null)
                    {
                        AddMessage(
                            ResultMessageType.Warning,
                            $"Elements of type {name} can't be children of element {node.Name} (path: {linkListNodeId})"
                        );
                        continue;
                    }

                    SetValue(type, property, obj, parsedNode);
                }
            }

            CheckRequiredProperties(type, obj, linkListNodeId);

            return obj;
        }

        private object ParseNode(XmlNode node, string linkListNodeId = null)
        {
            switch (node.NodeType)
            {
                case XmlNodeType.Element:
                    return ParseElement(node, linkListNodeId);

                case XmlNodeType.XmlDeclaration:
                    return CheckXMLDeclaration(node);

                case XmlNodeType.SignificantWhitespace:
                case XmlNodeType.Whitespace:
                    return null;

                case XmlNodeType.Text:
                    return ParseText(node);

                case XmlNodeType.CDATA:
                    return ParseCDATA(node);

                case XmlNodeType.Entity:
                    return ParseEntity(node);

                case XmlNodeType.Comment:
                    return ParseComment(node);

                case XmlNodeType.Document:
                case XmlNodeType.DocumentFragment:
                case XmlNodeType.Attribute:
                case XmlNodeType.EndElement:
                case XmlNodeType.EndEntity:
                case XmlNodeType.EntityReference:
                case XmlNodeType.None:
                case XmlNodeType.Notation:
                case XmlNodeType.ProcessingInstruction:
                default:
                    AddMessage(ResultMessageType.Error, $"Found invalid Element in XML. Type: {node.NodeType}, Content: {node.OuterXml}");
                    return null;

            }



        }


    }
}
