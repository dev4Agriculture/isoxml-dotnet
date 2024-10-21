using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using Dev4Agriculture.ISO11783.ISOXML.Messaging;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.Utils;

namespace Dev4Agriculture.ISO11783.ISOXML.Serializer
{
    public class IsoxmlSerializer
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
            }},
            {"DateTime", value =>
            {
                var date = DateTime.Parse(value);
                return date;
            } }
        };

        private readonly Assembly _isoxmlAssembly;

        private ResultWithMessages<object> _result = new ResultWithMessages<object>();

        public IsoxmlSerializer()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            _isoxmlAssembly = assemblies.LastOrDefault(assembly => assembly.GetName().Name == "Dev4Agriculture.ISO11783.ISOXML");

        }
        public ResultWithMessages<object> Deserialize(XmlDocument xml)
        {
            _result = new ResultWithMessages<object>();
            var keepCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            var index = -1;
            object parsed = null;
            while (parsed == null && index < xml.ChildNodes.Count)
            {
                index++;
                parsed = ParseNode(xml.ChildNodes[index], $"{xml.ChildNodes[index].Name}[0]");
            }
            _result.SetResult(parsed);
            Thread.CurrentThread.CurrentCulture = keepCulture;
            return _result;
        }

        public void Serialize(ISO11783TaskDataFile taskData, string path)
        {
            //Create our own namespaces for the output
            var ns = new XmlSerializerNamespaces();

            //Add an empty namespace and empty value
            ns.Add("", "");
            var xmlWriterSettings = new XmlWriterSettings() { Indent = true };
            var ser = new XmlSerializer(typeof(ISO11783TaskDataFile));
            using var xmlWriter = XmlWriter.Create(path, xmlWriterSettings);
            ser.Serialize(xmlWriter, taskData, ns);
        }

        public string Serialize(object obj)
        {
            //Create our own namespaces for the output
            var ns = new XmlSerializerNamespaces();

            //Add an empty namespace and empty value
            ns.Add("", "");
            var xmlWriterSettings = new XmlWriterSettings() { Indent = true, Encoding = Encoding.UTF8 };
            var ser = new XmlSerializer(obj.GetType());
            var output = new StringWriterWithEncoding();
            using var xmlWriter = XmlWriter.Create(output, xmlWriterSettings);
            ser.Serialize(xmlWriter, obj, ns);
            return output.ToString();
        }

        public ISO11783TaskDataFile DeepClone(ISO11783TaskDataFile taskData)
        {
            var xmlWriterSettings = new XmlWriterSettings() { Indent = true };
            var ser = new XmlSerializer(typeof(ISO11783TaskDataFile));
            using var stream = new MemoryStream();
            using var xmlWriter = XmlWriter.Create(stream, xmlWriterSettings);
            ser.Serialize(xmlWriter, taskData);
            var xmlDoc = new XmlDocument();
            stream.Position = 0;
            xmlDoc.Load(stream);
            var copiedTask = Deserialize(xmlDoc);
            return (ISO11783TaskDataFile)copiedTask.Result;
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

        private Type FindType(string name)
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


        private void ValidateProperty(PropertyInfo property, object value, string attrValue, string isoxmlNodeId)
        {
            var rangeAttr = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.RangeAttribute>();
            var maxLengthAttr = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.MaxLengthAttribute>();
            var minLengthAttr = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.MinLengthAttribute>();
            var regexAttr = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.RegularExpressionAttribute>();
            if (rangeAttr != null && !rangeAttr.IsValid(value))
            {
                _result.AddWarning(ResultMessageCode.XSDAttributeValueRange,
                    ResultDetail.FromString(property.Name),
                    ResultDetail.FromString(rangeAttr.Minimum.ToString()),
                    ResultDetail.FromString(rangeAttr.Maximum.ToString()),
                    ResultDetail.FromId(isoxmlNodeId),
                    ResultDetail.FromString(value.ToString())
                );
            }

            if (maxLengthAttr != null && !maxLengthAttr.IsValid(value))
            {
                _result.AddWarning(ResultMessageCode.XSDAttributeValueTooLong,
                    ResultDetail.FromString(attrValue),
                    ResultDetail.FromNumber(maxLengthAttr.Length),
                    ResultDetail.FromString(property.Name),
                    ResultDetail.FromId(isoxmlNodeId)
                );
            }

            if (minLengthAttr != null && !minLengthAttr.IsValid(value))
            {
                _result.AddWarning(ResultMessageCode.XSDAttributeValueTooShort,
                    ResultDetail.FromString(attrValue),
                    ResultDetail.FromNumber(minLengthAttr.Length),
                    ResultDetail.FromString(property.Name),
                    ResultDetail.FromId(isoxmlNodeId)
                );
            }

            if (regexAttr != null && !regexAttr.IsValid(attrValue))
            {
                _result.AddWarning(ResultMessageCode.XSDAttributeRegExMissmatch,
                    ResultDetail.FromString(attrValue),
                    ResultDetail.FromString(regexAttr.Pattern),
                    ResultDetail.FromString(property.Name),
                    ResultDetail.FromId(isoxmlNodeId)
                );
            }
        }

        private void CheckRequiredProperties(Type type, object obj, string isoxmlNodeId)
        {
            foreach (var property in type.GetProperties())
            {
                var required = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.RequiredAttribute>() != null;

                if (required && property.GetValue(obj) == null)
                {
                    _result.AddWarning(ResultMessageCode.XSDAttributeRequired,
                        ResultDetail.FromString(property.Name),
                        ResultDetail.FromId(isoxmlNodeId));
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
            _result.AddError(ResultMessageCode.XMLWrongElement, ResultDetail.FromString(node.OuterXml));
            return null;
        }

        private object ParseEntity(XmlNode node)
        {
            _result.AddError(ResultMessageCode.XMLWrongElement, ResultDetail.FromString(node.OuterXml));
            return null;
        }

        private object ParseComment(XmlNode node)
        {
            _result.AddWarning(ResultMessageCode.XMLCommentFound, ResultDetail.FromString(node.InnerText));
            return null;
        }


        private object ParseText(XmlNode node)
        {
            _result.AddError(ResultMessageCode.XMLTextIn, ResultDetail.FromString(node.InnerText));
            return null;
        }

        private object ParseElement(XmlNode node, string isoxmlNodeId = null)
        {
            var type = FindType(node.Name);
            if (type == null)
            {
                var isRoot = string.IsNullOrEmpty(isoxmlNodeId);
                if (isRoot)
                {
                    _result.AddError(ResultMessageCode.XSDAttributeUnknown,
                        ResultDetail.FromString(node.Name),
                        ResultDetail.FromId(isoxmlNodeId));
                }
                else
                {
                    _result.AddWarning(ResultMessageCode.XSDAttributeUnknown,
                        ResultDetail.FromString(node.Name),
                        ResultDetail.FromId(isoxmlNodeId));

                }
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
                        _result.AddWarning(ResultMessageCode.XSDAttributeUnknown,
                        ResultDetail.FromString(attr.Name),
                        ResultDetail.FromId(isoxmlNodeId));
                    }
                    continue;
                }

                if (property.PropertyType.IsEnum)
                {
                    var enumValue = GetEnumValue(property.PropertyType, attr.Value);

                    if (enumValue == null)
                    {
                        _result.AddWarning(ResultMessageCode.XSDEnumUnknown,
                            ResultDetail.FromString(attr.Value),
                            ResultDetail.FromString(property.Name),
                            ResultDetail.FromId(isoxmlNodeId)
                        );
                        continue;
                    }

                    property.SetValue(obj, Enum.Parse(property.PropertyType, enumValue));
                    if (property.Name.EndsWith("Value"))
                    {
                        var propToSet = type.GetProperty(property.Name.Substring(0, property.Name.LastIndexOf("Value")));
                        propToSet?.SetValue(obj, Enum.Parse(property.PropertyType, enumValue));
                    }
                }
                else
                {
                    Convertors.TryGetValue(property.PropertyType.Name, out var convertor);
                    try
                    {
                        var convertedAttr = convertor(attr.Value);
                        property.SetValue(obj, convertedAttr);
                        if (property.Name.EndsWith("Value"))
                        {
                            var propToSet = type.GetProperty(property.Name.Substring(0, property.Name.LastIndexOf("Value")));
                            propToSet?.SetValue(obj, convertedAttr);
                        }
                        ValidateProperty(property, convertedAttr, attr.Value, isoxmlNodeId);
                    }
                    catch (Exception e)
                    {
                        _result.AddWarning(ResultMessageCode.XSDAttributeParsing,
                            ResultDetail.FromString(attr.Value),
                            ResultDetail.FromString(e.GetType().ToString()),
                            ResultDetail.FromString(e.Message),
                            ResultDetail.FromString(property.Name),
                            ResultDetail.FromId(isoxmlNodeId)
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
                var childNodeIsoxmlId = $"{isoxmlNodeId}->{name}[{count}]";

                var parsedNode = ParseNode(childNode, childNodeIsoxmlId);

                if (parsedNode != null)
                {
                    var property = GetPropertyByElementName(type, name);
                    if (property == null)
                    {
                        _result.AddWarning(ResultMessageCode.XSDElementWrongChild,
                            ResultDetail.FromString(name),
                            ResultDetail.FromString(node.Name),
                            ResultDetail.FromId(isoxmlNodeId));
                        continue;
                    }

                    SetValue(type, property, obj, parsedNode);
                }
            }

            CheckRequiredProperties(type, obj, isoxmlNodeId);

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
                    _result.AddError(ResultMessageCode.XSDElementInvalid,
                        ResultDetail.FromString(node.NodeType.ToString()),
                        ResultDetail.FromString(node.Value));
                    return null;

            }
        }
    }
}
