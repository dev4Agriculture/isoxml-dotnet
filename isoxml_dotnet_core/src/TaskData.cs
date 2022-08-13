using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Dev4ag.ISO11783.TaskFile;

namespace Dev4ag {
    internal class TaskData {
        private static IsoxmlSerializer isoxmlSerializer = new IsoxmlSerializer();
        internal static string fixTaskDataPath(string path)
        {
            if (path.ToUpper().EndsWith(".XML") == false)
            {
                path = Path.Combine(path.ToString(), "TASKDATA.XML");
            }
            return path;
        }

        
        public static ResultWithMessages<ISO11783TaskDataFile> ParseTaskData(string isoxmlString, string path) {
            ISO11783TaskDataFile taskData = null;
            var messages = new List<ResultMessage>();
            try {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(isoxmlString);
                //Check for XFR External FileReferences here as we need to merge everything before Deserialization
                XmlNodeList externals = xmlDoc.GetElementsByTagName("XFR");
                foreach ( XmlNode element in externals)
                {
                    try
                    {
                        var extPath = path.Replace("TASKDATA.XML", element.Attributes["A"].Value + ".XML");
                        var extContent = File.ReadAllText(extPath.ToString());
                        XmlDocument externalDoc = new XmlDocument();
                        externalDoc.LoadXml(extContent);
                        MergeExternalContent(xmlDoc, externalDoc);
                    }
                    catch (FileNotFoundException ex)
                    {
                        messages.Add(new ResultMessage(ResultMessageType.Error, "External file missing: " + element.Attributes["A"].Value));
                    }
                    catch (IOException ex)
                    {
                        messages.Add(new ResultMessage(ResultMessageType.Error, "External file missing or inaccessible: " + element.Attributes["A"].Value));
                    }
                    catch (Exception ex) 
                    {
                        messages.Add(new ResultMessage(ResultMessageType.Error, "External file invalid: " + element.Attributes["A"].Value));
                    }
                }
                taskData = (ISO11783TaskDataFile)isoxmlSerializer.Deserialize(xmlDoc);

                messages.AddRange(isoxmlSerializer.messages);
            } catch(Exception ex) {
                messages.Add(new ResultMessage(ResultMessageType.Error, ex.Message));
            }
            return new ResultWithMessages<ISO11783TaskDataFile>(taskData, messages);
        }

        private static XmlDocument MergeExternalContent(XmlDocument taskData, XmlDocument externalFile)
        {
            if (externalFile != null && taskData!=null)
            {
                var rootNode = externalFile.FirstChild;
                foreach (XmlNode entry in rootNode.ChildNodes)
                {
                    var imported = taskData.ImportNode(entry,true);
                    taskData.GetElementsByTagName("ISO11783_TaskData").Item(0).AppendChild(imported);
                }
            }
            return taskData;

        }

        public static ResultWithMessages<ISO11783TaskDataFile> LoadTaskData(string path)
        {
            var messages = new List<ResultMessage>();
            path = fixTaskDataPath(path);
            if(File.Exists(path) == false)
            {
                messages.Add(new ResultMessage(ResultMessageType.Error, "TASKDATA.XML not found!"));
            }
            string text = File.ReadAllText(path.ToString());
            var result = ParseTaskData(text,path);
            return result;
        }


        public static void SaveTaskData(ISO11783TaskDataFile taskData, string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path = fixTaskDataPath(path);
            var isoxmlSerializer = new IsoxmlSerializer();
            isoxmlSerializer.Serialize(taskData, path);
        }

    }
}