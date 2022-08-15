using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
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

namespace Dev4Agriculture.ISO11783.ISOXML
{
    public class IsoxmlSerializer
    {

        private delegate object ValueConvertor(string value);


        static private Dictionary<string, ValueConvertor> _convertors = new Dictionary<string, ValueConvertor>() {
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
                    byte[] data = new byte[value.Length / 2];
                    for (int index = 0; index < data.Length; index++)
                    {
                        string byteValue = value.Substring(index * 2, 2);
                        data[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    }
                    return data;
                } catch (Exception) {
                    throw new Exception($"Can not parse value '{value}'");
                }
            }},
            {"DateTime", value =>
            {
                var date = DateTime.Parse(value);
                return date;
            } }
        };

        private Assembly _isoxmlAssembly;

        public List<ResultMessage> messages = new List<ResultMessage>();

        public IsoxmlSerializer()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            this._isoxmlAssembly = assemblies.FirstOrDefault(assembly => assembly.GetName().Name == "isoxml_dotnet_core");

        }
        public object Deserialize(XmlDocument xml)
        {
            messages.Clear();
            var keepCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            object result = null;
            int index = -1;
            while (result == null && index < xml.ChildNodes.Count)
            {
                index++;
                result = ParseNode(xml.ChildNodes[index], $"{xml.ChildNodes[index].Name}[0]");
            }
            Thread.CurrentThread.CurrentCulture = keepCulture;
            return result;
        }


        public void Serialize(ISO11783TaskDataFile taskData, string path)
        {
            XmlSerializer ser = new XmlSerializer(typeof(ISO11783TaskDataFile));
            TextWriter writer = new StreamWriter(path);
            ser.Serialize(writer, taskData);
            writer.Close();
        }
        // mainly for debugging
        public HashSet<string> GetAllAttrTypes()
        {
            return _isoxmlAssembly.GetTypes()
                .Where(type => type.Namespace == Constants.ISOXMLClassName + ".TaskFile")
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

        private System.Type findType(string name)
        {
            foreach (var type in _isoxmlAssembly.GetTypes())
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

        private PropertyInfo getPropertyByAttrName(System.Type type, string xmlAttrName)
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

        private PropertyInfo getPropertyByElementName(System.Type type, string xmlElementName)
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

        private string getEnumValue(System.Type enumType, string xmlValue)
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
        private void setValue(System.Type type, PropertyInfo property, object obj, object value)
        {
            foreach (var implInterface in property.PropertyType.GetInterfaces())
            {
                if (implInterface.Name == "IList")
                {
                    IList list = (IList)property.GetValue(obj);
                    list.Add(value);
                    return;
                }
            }
            property.SetValue(obj, value);
        }

        private void addMessage(ResultMessageType type, string message)
        {
            messages.Add(new ResultMessage(type, message));
        }

        private void validateProperty(PropertyInfo property, object value, string attrValue, string isoxmlNodeId)
        {
            var rangeAttr = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.RangeAttribute>();
            var maxLengthAttr = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.MaxLengthAttribute>();
            var minLengthAttr = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.MinLengthAttribute>();
            var regexAttr = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.RegularExpressionAttribute>();
            if (rangeAttr != null && !rangeAttr.IsValid(value))
            {
                addMessage(
                    ResultMessageType.Warning,
                    $"The field {property.Name} must be between {rangeAttr.Minimum} and {rangeAttr.Maximum} (path: {isoxmlNodeId}; value: {value})"
                );
            }

            if (maxLengthAttr != null && !maxLengthAttr.IsValid(value))
            {
                addMessage(
                    ResultMessageType.Warning,
                    $"The field {property.Name} has length more than {maxLengthAttr.Length} (path: {isoxmlNodeId}; value: {attrValue})"
                );
            }

            if (minLengthAttr != null && !minLengthAttr.IsValid(value))
            {
                addMessage(
                    ResultMessageType.Warning,
                    $"The field {property.Name} has length less than {minLengthAttr.Length} (path: {isoxmlNodeId}; value: {attrValue})"
                );
            }

            if (regexAttr != null && !regexAttr.IsValid(attrValue))
            {
                addMessage(
                    ResultMessageType.Warning,
                    $"The field {property.Name} doesn't match regular expression {regexAttr.Pattern} (path: {isoxmlNodeId}; value: {attrValue})"
                );
            }
        }

        private void checkRequiredProperties(System.Type type, object obj, string isoxmlNodeId)
        {
            foreach (var property in type.GetProperties())
            {
                var required = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.RequiredAttribute>() != null;

                if (required && property.GetValue(obj) == null)
                {
                    addMessage(
                        ResultMessageType.Warning,
                        $"Missing required property {property.Name} (path: {isoxmlNodeId})"
                    );
                }
            }
        }

        private object CheckXMLDeclaration(XmlNode node)
        {
            return null;
        }


        private object ParseCDATA(XmlNode node)
        {
            addMessage(ResultMessageType.Error, "ISOXML includes CDATA-Element which is not allowed: " + node.OuterXml);
            return null;
        }

        private object ParseEntity(XmlNode node)
        {
            addMessage(ResultMessageType.Error, "ISOXML includes Entity-Element which is not allowed: " + node.OuterXml);
            return null;
        }

        private object ParseComment(XmlNode node)
        {
            addMessage(ResultMessageType.Warning, "Comment found: " + node.InnerText);
            return null;
        }


        private object ParseText(XmlNode node)
        {
            addMessage(ResultMessageType.Error, "ISOXML includes Text-Element which is not allowed: " + node.InnerText);
            return null;
        }

        private object ParseElement(XmlNode node, string isoxmlNodeId = null)
        {
            var type = findType(node.Name);
            if (type == null)
            {
                var isRoot = String.IsNullOrEmpty(isoxmlNodeId);
                addMessage(
                    isRoot ? ResultMessageType.Error : ResultMessageType.Warning,
                    $"Unknown XML element {node.Name} (path: {isoxmlNodeId})"
                );
                return null;
            }
            var obj = Activator.CreateInstance(type);
            foreach (XmlAttribute attr in node.Attributes)
            {
                var property = getPropertyByAttrName(type, attr.Name);

                if (property == null)
                {
                    //TODO there should be a more generic way for this.
                    //Ignore XSD Schemata information
                    if (!attr.Name.Equals("xmlns:xsi") && !attr.Name.Equals("xmlns:xsd"))
                    {
                        addMessage(
                            ResultMessageType.Warning,
                            $"Unknown XML attribute {attr.Name} (path: {isoxmlNodeId})"
                        );
                    }
                    continue;
                }

                if (property.PropertyType.IsEnum)
                {
                    var enumValue = getEnumValue(property.PropertyType, attr.Value);

                    if (enumValue == null)
                    {
                        addMessage(
                            ResultMessageType.Warning,
                            $"Unknown enum value {attr.Value} (path: {isoxmlNodeId}; property: {property.Name})"
                        );
                        continue;
                    }

                    property.SetValue(obj, Enum.Parse(property.PropertyType, enumValue));
                }
                else
                {
                    ValueConvertor convertor = null;
                    _convertors.TryGetValue(property.PropertyType.Name, out convertor);
                    try
                    {
                        var convertedAttr = convertor(attr.Value);
                        property.SetValue(obj, convertedAttr);
                        validateProperty(property, convertedAttr, attr.Value, isoxmlNodeId);
                    }
                    catch (Exception e)
                    {
                        addMessage(
                            ResultMessageType.Warning,
                            $"Cannot parse value {attr.Value} (path: {isoxmlNodeId}; property: {property.Name}), Error: {e.GetType()} Message:{e.Message}"
                        );
                    }
                }
            }

            var childrenCount = new Dictionary<string, int>();

            foreach (XmlNode childNode in node.ChildNodes)
            {

                string name = childNode.Name;
                int count;
                if (childrenCount.TryGetValue(name, out count))
                {
                    childrenCount[name] = count + 1;
                }
                else
                {
                    count = 0;
                    childrenCount.Add(name, count + 1);
                }
                var childNodeIsoxmlId = $"{isoxmlNodeId}->{name}[{count}]";

                var parsedNode = ParseNode(childNode, childNodeIsoxmlId);

                if (parsedNode != null)
                {
                    var property = getPropertyByElementName(type, name);
                    if (property == null)
                    {
                        addMessage(
                            ResultMessageType.Warning,
                            $"Elements of type {name} can't be children of element {node.Name} (path: {isoxmlNodeId})"
                        );
                        continue;
                    }

                    setValue(type, property, obj, parsedNode);
                }
            }

            checkRequiredProperties(type, obj, isoxmlNodeId);

            return obj;
        }

        private object ParseNode(XmlNode node, string isoxmlNodeId = null)
        {
            switch (node.NodeType)
            {
                case XmlNodeType.Element:
                    return ParseElement(node, isoxmlNodeId);

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
                    addMessage(ResultMessageType.Error, $"Found invalid Element in XML. Type: {node.NodeType.ToString()}, Content: {node.OuterXml}");
                    return null;

            }



        }


    }
}