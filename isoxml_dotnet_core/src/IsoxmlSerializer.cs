using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Dev4ag {
    public class IsoxmlSerializer {

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
                    throw new ArgumentException();
                }

                byte[] data = new byte[value.Length / 2];
                for (int index = 0; index < data.Length; index++)
                {
                    string byteValue = value.Substring(index * 2, 2);
                    data[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                }

                return data;
            }}
        };

        private Assembly _isoxmlAssembly;

        public IsoxmlSerializer() {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies) {
                if (assembly.GetName().Name == "isoxml_dotnet_core") {
                    this._isoxmlAssembly = assembly;
                    break;
                }
            }
        }
        public object Deserialize(XmlDocument xml) {

            return ParseNode(xml.FirstChild);
        }
        public HashSet<string> GetAllAttrTypes() {
            return _isoxmlAssembly.GetTypes()
                .Where(type => type.Namespace == "Dev4ag.ISO11783.TaskFile")
                .SelectMany(type => type.GetProperties()
                    .Where(property => property.CustomAttributes.FirstOrDefault(
                        attr => attr.AttributeType.FullName == "System.Xml.Serialization.XmlAttributeAttribute"
                    ) != null && !property.PropertyType.IsEnum)
                    .Select(property => property.PropertyType.Name)
                ).Aggregate(new HashSet<string>(), (res, typeName) => {
                    res.Add(typeName);
                    return res;
                });
        }

        private Type findType(string name) {
            foreach (var type in _isoxmlAssembly.GetTypes()) {
                foreach (var attr in type.CustomAttributes) {
                    if (attr.AttributeType.FullName == "System.Xml.Serialization.XmlTypeAttribute" &&
                        (string)attr.ConstructorArguments[0].Value == name
                    ) {
                        return type;
                    }
                }
            }
            return null;
        }

        private PropertyInfo getPropertyByAttrName(Type type, string xmlAttrName) {
            foreach (var property in type.GetProperties()) {
                foreach (var attr in property.CustomAttributes) {
                    if (attr.AttributeType.FullName == "System.Xml.Serialization.XmlAttributeAttribute" &&
                        (string)attr.ConstructorArguments[0].Value == xmlAttrName
                    ) {
                        return property;
                    }
                }
            }
            return null;
        }

        private PropertyInfo getPropertyByElementName(Type type, string xmlElementName) {
            foreach (var property in type.GetProperties()) {
                foreach (var attr in property.CustomAttributes) {
                    if (attr.AttributeType.FullName == "System.Xml.Serialization.XmlElementAttribute" &&
                        (string)attr.ConstructorArguments[0].Value == xmlElementName
                    ) {
                        return property;
                    }
                }
            }
            return null;
        }

        private string getEnumValue(Type enumType, string xmlValue) {
            var enumValueMembers = enumType.GetMembers(
                BindingFlags.Public | BindingFlags.Static
            );

            foreach (var value in enumValueMembers) {
                foreach (var enumValueAttr in value.CustomAttributes) {
                    if (enumValueAttr.AttributeType.FullName == "System.Xml.Serialization.XmlEnumAttribute" &&
                        (string)enumValueAttr.ConstructorArguments[0].Value == xmlValue
                    ) {
                        return value.Name;
                    }
                }
            }

            return null;
        }

        private void setValue(Type type, PropertyInfo property, object obj, object value) {
            foreach (var implInterface in property.PropertyType.GetInterfaces()) {
                if (implInterface.Name == "IList") {
                    IList list = (IList)property.GetValue(obj);
                    list.Add(value);
                    return;
                }
            }
            property.SetValue(obj, value);
        }

        private object ParseNode(XmlNode node) {
            var type = findType(node.Name);
            var obj = Activator.CreateInstance(type);
            foreach (XmlAttribute attr in node.Attributes) {
                var property = getPropertyByAttrName(type, attr.Name);

                if (property.PropertyType.IsEnum) {
                    var enumValue = getEnumValue(property.PropertyType, attr.Value);
                    property.SetValue(obj, Enum.Parse(property.PropertyType, enumValue));
                } else {
                    ValueConvertor convertor = null;
                    _convertors.TryGetValue(property.PropertyType.Name, out convertor);

                    if (convertor == null) {
                        Console.WriteLine($"Unknown type: {property.PropertyType.Name}");
                    } else {
                        var convertedAttr = convertor(attr.Value);
                        property.SetValue(obj, convertedAttr);
                    }
                }
            }

            foreach (XmlNode childNode in node.ChildNodes) {
                var property = getPropertyByElementName(type, childNode.Name);
                var parsedNode = ParseNode(childNode);
                setValue(type, property, obj, parsedNode);
            }

            return obj;
        }
    }
}