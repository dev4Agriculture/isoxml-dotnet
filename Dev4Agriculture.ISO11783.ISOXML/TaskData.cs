using System;
using System.IO;
using System.Linq;
using System.Xml;
using Dev4Agriculture.ISO11783.ISOXML.Messaging;
using Dev4Agriculture.ISO11783.ISOXML.Serializer;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.Utils;

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
            var result = new ResultWithMessages<ISO11783TaskDataFile>();
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
                        var mergeResult = MergeExternalContent(xmlDoc, externalDoc);
                        if (mergeResult.Messages.Count > 0)
                        {
                            result.Messages.AddRange(mergeResult.Messages);
                        };
                    }
                    catch (FileNotFoundException)
                    {
                        result.AddError(ResultMessageCode.FileNotFound,
                            ResultDetail.FromString(element.Attributes["A"].Value));
                    }
                    catch (IOException ex)
                    {
                        result.AddError(ResultMessageCode.FileAccessImpossible,
                            ResultDetail.FromString(element.Attributes["A"].Value),
                            ResultDetail.FromString(ex.Message));
                    }
                    catch (Exception ex)
                    {
                        result.AddError(ResultMessageCode.FileInvalid,
                            ResultDetail.FromString(element.Attributes["A"].Value),
                            ResultDetail.FromString(ex.Message));
                    }
                }
                var deserialized = IsoxmlSerializer.Deserialize(xmlDoc);
                result.AddMessages(deserialized.Messages);
                result.SetResult((ISO11783TaskDataFile)deserialized.Result);
            }
            catch (Exception ex)
            {
                result.AddError(ResultMessageCode.Unknown, ResultDetail.FromString(ex.Message));
            }
            return result;
        }

        private static ResultWithMessages<XmlDocument> MergeExternalContent(XmlDocument taskData, XmlDocument externalFile)
        {
            if (externalFile != null && taskData != null)
            {
                var rootNode = externalFile.GetElementsByTagName("XFC").Item(0);
                if (rootNode == null)
                {
                    var resultWithMessages = new ResultWithMessages<XmlDocument>(taskData);
                    resultWithMessages.Messages.Add(ResultMessage.Error(ResultMessageCode.XMLXFCNotFound));
                    return resultWithMessages;
                }
                foreach (XmlNode entry in rootNode.ChildNodes)
                {
                    var imported = taskData.ImportNode(entry, true);
                    taskData.GetElementsByTagName("ISO11783_TaskData").Item(0).AppendChild(imported);
                }
            }
            return new ResultWithMessages<XmlDocument>(taskData);

        }

        /// <summary>
        /// Loads a TASKDATA into an ISOXML Object. First checks, if the file itself exists, otherwise the Function assumes it was provided with a Directory Path and has to add TASKDATA.XML
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static ResultWithMessages<ISO11783TaskDataFile> LoadTaskData(string path)
        {
            var text = "";
            var filePath = "";
            var result = new ResultWithMessages<IsoLinkList>();
            if (!File.Exists(path))
            {
                var fileName = "TASKDATA.XML";
                if (FileUtils.HasMultipleFilesEndingWithThatName(path, fileName))
                {
                    result.AddWarning(ResultMessageCode.FileNameEndingMultipleTimes, ResultDetail.FromString(fileName));
                }

                if (!FileUtils.AdjustFileNameToIgnoreCasing(path, fileName, out filePath))
                {
                    return new ResultWithMessages<ISO11783TaskDataFile>(
                        new ISO11783TaskDataFile(),
                        ResultMessage.Error(
                            ResultMessageCode.FileNotFound,
                            ResultDetail.FromPath(path)
                            )
                        );
                }
                text = File.ReadAllText(filePath);
            }
            else
            {
                text = File.ReadAllText(path);

            }

            var toReturn = ParseTaskData(text, !string.IsNullOrWhiteSpace(filePath) ? filePath : path);
            toReturn.AddMessages(result.Messages);
            return toReturn;
        }


        public static void SaveTaskData(ISO11783TaskDataFile taskData, string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path = FixTaskDataPath(path);
            FixTaskDataContent(taskData);
            var isoxmlSerializer = new IsoxmlSerializer();
            isoxmlSerializer.Serialize(taskData, path);
        }


        /// <summary>
        /// Some Elements within the Taskdata.xml may only have up to x (currently 9) digits. That's what we fix here
        /// </summary>
        /// <param name="taskData"></param>
        private static void FixTaskDataContent(ISO11783TaskDataFile taskData)
        {
            taskData.Partfield.ToList().ForEach(entry => entry.FixPositionDigits());
            taskData.BaseStation.ToList().ForEach(entry => entry.FixPositionDigits());
            foreach (var task in taskData.Task)
            {
                task.Grid.FirstOrDefault()?.FixPositionDigits();
                task.Time.ToList().ForEach(entry => entry.FixPositionDigits());
                task.CommentAllocation.ToList().ForEach(entry => entry.AllocationStamp?.FixPositionDigits());
                task.DeviceAllocation.ToList().ForEach(entry => entry.AllocationStamp?.FixPositionDigits());
                task.ProductAllocation.ToList().ForEach(entry => entry.AllocationStamp?.FixPositionDigits());
                task.WorkerAllocation.ToList().ForEach(entry => entry.AllocationStamp?.FixPositionDigits());
                task.DeviceAllocation.ToList().ForEach(entry => entry.AllocationStamp?.FixPositionDigits());
                task.DeviceAllocation.ToList().ForEach(entry => entry.AllocationStamp?.FixPositionDigits());
                task.Time.ToList().ForEach(entry => entry.Position.ToList().ForEach(ptn => ptn.FixDigits()));


            }

        }

        public static ResultWithMessages<ISO11783TaskDataFile> FromParsedElement(string elementString)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(elementString);
            var element = IsoxmlSerializer.Deserialize(xmlDoc);
            var result = new ResultWithMessages<ISO11783TaskDataFile>()
            {
                Messages = element.Messages
            };
            if (element.Result is ISO11783TaskDataFile file)
            {
                result.SetResult((ISO11783TaskDataFile)element.Result);
                return result;
            }

            //If the object is not a whole ISOXML TaskData, we return an TaskDataFile that has one element added
            var taskData = new ISO11783TaskDataFile();
            switch (element.Result)
            {
                case ISOAttachedFile el:
                    taskData.AttachedFile.Add(el);
                    break;
                case ISOBaseStation el:
                    taskData.BaseStation.Add(el);
                    break;
                case ISOCodedCommentGroup el:

                    taskData.CodedCommentGroup.Add(el);
                    break;
                case ISOCodedComment el:
                    taskData.CodedComment.Add(el);
                    break;
                case ISOColourLegend el:
                    taskData.ColourLegend.Add(el);
                    break;
                case ISOCropType el:
                    taskData.CropType.Add(el);
                    break;
                case ISOCulturalPractice el:
                    taskData.CulturalPractice.Add(el);
                    break;
                case ISOCustomer el:
                    taskData.Customer.Add(el);
                    break;
                case ISODevice el:
                    taskData.Device.Add(el);
                    break;
                case ISOFarm el:
                    taskData.Farm.Add(el);
                    break;
                case ISOOperationTechnique el:
                    taskData.OperationTechnique.Add(el);
                    break;
                case ISOPartfield el:
                    taskData.Partfield.Add(el);
                    break;
                case ISOProduct el:
                    taskData.Product.Add(el);
                    break;
                case ISOProductGroup el:
                    taskData.ProductGroup.Add(el);
                    break;
                case ISOTask el:
                    taskData.Task.Add(el);
                    break;
                case ISOValuePresentation el:
                    taskData.ValuePresentation.Add(el);
                    break;
                case ISOWorker el:
                    taskData.Worker.Add(el);
                    break;
            }
            result.SetResult(taskData);
            return result;
        }
    }
}
