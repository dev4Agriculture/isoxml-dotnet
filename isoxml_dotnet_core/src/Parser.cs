using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Xml.Serialization;
using Dev4ag.ISO11783.TaskFile;

namespace Dev4ag {
    public class ResultWithWarnings<ResultType> where ResultType: class {
        public ResultType result = null;
        public List<string> warnings = new List<string>();

        public ResultWithWarnings(ResultType result, List<string> warnings) {
            this.result = result;
            this.warnings = warnings;
        }
        public ResultWithWarnings(ResultType result) {
            this.result = result;
        }
    }
    public class ISOXMLParser {
        public static ResultWithWarnings<Device> ParseDeviceDescription(string xmlDeviceDescription) {
            Device device = null;
            var warnings = new List<string>();
            try {
                XmlSerializer serializer = new XmlSerializer(typeof(Device));
                using ( StringReader reader = new StringReader(xmlDeviceDescription))
                {
                    device = (Device)serializer.Deserialize(reader);
                }
                var context = new ValidationContext(device, serviceProvider: null, items: null);
                var validationResults = new List<ValidationResult>();

                bool isValid = Validator.TryValidateObject(device, context, validationResults, true);

                if (!isValid) {
                    validationResults.ForEach(result => {
                        warnings.Add(result.ErrorMessage);
                    });
                }
            } catch(Exception ex) {
                warnings.Add(ex.ToString());
            }
            return new ResultWithWarnings<Device>(device, warnings);
        }
    }
}