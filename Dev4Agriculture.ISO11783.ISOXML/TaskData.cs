using Dev4Agriculture.ISO11783.ISOXML.Messaging;
using Dev4Agriculture.ISO11783.ISOXML.Serializer;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Dev4Agriculture.ISO11783.ISOXML
{
    internal class TaskData
    {
        private static readonly IsoxmlSerializer IsoxmlSerializer = new IsoxmlSerializer();
        internal static string FixTaskDataPath(string path)
        {
            if (path.ToUpper().EndsWith(".XML") == false)
            {
                path = Path.Combine(path.ToString(), "TASKDATA.XML");
            }
            return path;
        }


        public static ResultWithMessages<ISO11783TaskDataFile> ParseTaskData(string isoxmlString, string path)
        {
            ISO11783TaskDataFile taskData = null;
            var messages = new List<ResultMessage>();
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(isoxmlString);
                //Check for XFR External FileReferences here as we need to merge everything before Deserialization
                var externals = xmlDoc.GetElementsByTagName("XFR");
                foreach (XmlNode element in externals)
                {
                    try
                    {
                        var extPath = path.Replace("TASKDATA.XML", element.Attributes["A"].Value + ".XML");
                        var extContent = File.ReadAllText(extPath.ToString());
                        var externalDoc = new XmlDocument();
                        externalDoc.LoadXml(extContent);
                        MergeExternalContent(xmlDoc, externalDoc);
                    }
                    catch (FileNotFoundException ex)
                    {
                        messages.Add(new ResultMessage(ResultMessageType.Error, "External file missing: " + element.Attributes["A"].Value + ", message: " + ex.Message));
                    }
                    catch (IOException ex)
                    {
                        messages.Add(new ResultMessage(ResultMessageType.Error, "External file missing or inaccessible: " + element.Attributes["A"].Value + ", message: " + ex.Message));
                    }
                    catch (Exception ex)
                    {
                        messages.Add(new ResultMessage(ResultMessageType.Error, "External file invalid: " + element.Attributes["A"].Value + ", message: " + ex.Message));
                    }
                }
                taskData = (ISO11783TaskDataFile)IsoxmlSerializer.Deserialize(xmlDoc);

                messages.AddRange(IsoxmlSerializer.Messages);
            }
            catch (Exception ex)
            {
                messages.Add(new ResultMessage(ResultMessageType.Error, ex.Message));
            }
            return new ResultWithMessages<ISO11783TaskDataFile>(taskData, messages);
        }

        private static XmlDocument MergeExternalContent(XmlDocument taskData, XmlDocument externalFile)
        {
            if (externalFile != null && taskData != null)
            {
                var rootNode = externalFile.FirstChild;
                foreach (XmlNode entry in rootNode.ChildNodes)
                {
                    var imported = taskData.ImportNode(entry, true);
                    taskData.GetElementsByTagName("ISO11783_TaskData").Item(0).AppendChild(imported);
                }
            }
            return taskData;

        }

        /// <summary>
        /// Loads a TASKDATA into an ISOXML Object. First checks, if the file itself exists, otherwise the Function assumes it was provided with a Directory Path and has to add TASKDATA.XML
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static ResultWithMessages<ISO11783TaskDataFile> LoadTaskData(string path)
        {
            var text = "";
            if (!File.Exists(path))
            {
                if (!Utils.AdjustFileNameToIgnoreCasing(path, "TASKDATA.XML", out var filePath))
                {
                    return new ResultWithMessages<ISO11783TaskDataFile>(
                        new ISO11783TaskDataFile(),
                        new ResultMessage(
                            ResultMessageType.Error,
                            "TASKDATA.XML not found in " + path
                            )
                        );
                }
                text = File.ReadAllText(filePath);
            }
            else
            {
                text = File.ReadAllText(path);

            }

            return ParseTaskData(text, path);
        }


        public static void SaveTaskData(ISO11783TaskDataFile taskData, string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path = FixTaskDataPath(path);
            var isoxmlSerializer = new IsoxmlSerializer();
            isoxmlSerializer.Serialize(taskData, path);
        }

    }
}
