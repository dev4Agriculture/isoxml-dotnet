using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Dev4ag.ISO11783.TaskFile;

namespace Dev4ag {
    public class ISOXMLParser {
        public class ResultMessage {

            public ResultMessage(string type, string title) {
                this.type = type;
                this.title = title;
            }
            public string type;
            public string title;
        }

        public class ResultWithMessages<ResultType> where ResultType: class {
            public ResultType result = null;
            public List<ResultMessage> messages = new List<ResultMessage>();

            public ResultWithMessages(ResultType result, List<ResultMessage> messages) {
                this.result = result;
                this.messages = messages;
            }
            public ResultWithMessages(ResultType result) {
                this.result = result;
            }
        }
        public static ResultWithMessages<Device> ParseDeviceDescription(string xmlDeviceDescription) {
            Device device = null;
            var messages = new List<ResultMessage>();
            try {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlDeviceDescription);
                var isoxmlSerializer = new IsoxmlSerializer();
                device = (Device)isoxmlSerializer.Deserialize(xmlDoc);

                var context = new ValidationContext(device, serviceProvider: null, items: null);
                var validationResults = new List<ValidationResult>();

                bool isValid = Validator.TryValidateObject(device, context, validationResults, true);

                if (!isValid) {
                    validationResults.ForEach(result => {
                        messages.Add(new ResultMessage("warning", result.ErrorMessage));
                    });
                }
            } catch(Exception ex) {
                messages.Add(new ResultMessage("error", ex.Message));
            }
            return new ResultWithMessages<Device>(device, messages);
        }
    }
}