using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dev4Agriculture.ISO11783.ISOXML.IdHandling;
using Dev4Agriculture.ISO11783.ISOXML.LinkListFile;
using Dev4Agriculture.ISO11783.ISOXML.Messaging;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.TimeLog;
//Alias Definitions

using AsyncTask = System.Threading.Tasks;//Used for Task as this is a duplicate word with ISOXML Task.
namespace Dev4Agriculture.ISO11783.ISOXML
{
    public class ISOXML
    {
        public Dictionary<string, ISOGridFile> Grids { get; private set; }
        public Dictionary<string, ISOTLG> TimeLogs { get; private set; }

        private uint _maxGridIndex = 0;
        public ISO11783TaskDataFile Data { get; private set; }
        public string FolderPath { get; private set; }
        public List<ResultMessage> Messages { get; private set; }
        public IdTable IdTable { get; private set; }
        public IsoLinkList LinkList { get; private set; }
        public bool HasLinkList { get; private set; }
        private bool _binaryLoaded;

        public ISO11783TaskDataFileVersionMajor VersionMajor
        {
            get => Data.VersionMajor;
            set
            {
                Data.VersionMajor = value;
                if (HasLinkList)
                {
                    LinkList.VersionMajor = (ISO11783LinkListFileVersionMajor)value;
                }
            }
        }

        public ISO11783TaskDataFileVersionMinor VersionMinor
        {
            get => Data.VersionMinor;
            set
            {
                Data.VersionMinor = value;
                if (HasLinkList)
                {
                    LinkList.VersionMinor = (ISO11783LinkListFileVersionMinor)value;
                }
            }
        }

        public string ManagementSoftwareManufacturer
        {
            get => Data.ManagementSoftwareManufacturer;
            set
            {
                Data.ManagementSoftwareManufacturer = value;
                if (HasLinkList)
                {
                    LinkList.ManagementSoftwareManufacturer = value;
                }
            }
        }

        public string ManagementSoftwareVersion
        {
            get => Data.ManagementSoftwareVersion;
            set
            {
                Data.ManagementSoftwareVersion = value;
                if (HasLinkList)
                {
                    LinkList.ManagementSoftwareVersion = value;
                }
            }
        }

        public string TaskControllerManufacturer
        {
            get => Data.TaskControllerManufacturer;
            set
            {
                Data.TaskControllerManufacturer = value;
                if (HasLinkList)
                {
                    LinkList.TaskControllerManufacturer = value;
                }
            }
        }

        public string TaskControllerVersion
        {
            get => Data.TaskControllerVersion;
            set
            {
                Data.TaskControllerVersion = value;
                if (HasLinkList)
                {
                    LinkList.TaskControllerVersion = value;
                }
            }
        }

        public ISO11783TaskDataFileDataTransferOrigin DataTransferOrigin
        {
            get => Data.DataTransferOrigin;
            set
            {
                Data.DataTransferOrigin = value;
                if (HasLinkList)
                {
                    LinkList.DataTransferOrigin = (ISO11783LinkListFileDataTransferOrigin)value;
                }
            }
        }

        /// <summary>
        ///  This generates an initial ISOXML Element. It does NOT Load any file
        /// </summary>
        /// <param name="path">The path where the ISOXML TaskSet should be stored</param>
        private ISOXML(string path)
        {
            Data = new ISO11783TaskDataFile
            {
                ManagementSoftwareManufacturer = "unknown",
                ManagementSoftwareVersion = "unknown",
                TaskControllerManufacturer = "unknown",
                TaskControllerVersion = "unknown",
                DataTransferOrigin = ISO11783TaskDataFileDataTransferOrigin.FMIS
            };
            Grids = new Dictionary<string, ISOGridFile>();
            TimeLogs = new Dictionary<string, ISOTLG>();
            FolderPath = path;
            Messages = new List<ResultMessage>();
            IdTable = new IdTable();
            LinkList = null;
            HasLinkList = false;
        }

        /// <summary>
        /// Generate a LinkList if none is available yet
        /// </summary>
        public void AddLinkList()
        {
            if (VersionMajor != ISO11783TaskDataFileVersionMajor.Version4)
            {
                throw new Exception("LinkList can be included started from Version 4");
            }

            if (HasLinkList == false)
            {
                LinkList = new IsoLinkList()
                {
                    DataTransferOrigin = (ISO11783LinkListFileDataTransferOrigin)Data.DataTransferOrigin,
                    ManagementSoftwareManufacturer = Data.ManagementSoftwareManufacturer,
                    ManagementSoftwareVersion = Data.ManagementSoftwareVersion,
                    TaskControllerManufacturer = Data.TaskControllerManufacturer,
                    TaskControllerVersion = Data.TaskControllerVersion,
                    VersionMajor = (ISO11783LinkListFileVersionMajor)Data.VersionMajor,
                    VersionMinor = (ISO11783LinkListFileVersionMinor)Data.VersionMinor
                };
                Data.AttachedFile.Add(new ISOAttachedFile()
                {
                    FileType = 1,
                    FilenameWithExtension = "LINKLIST.XML",
                    ManufacturerGLN = ""
                });
                HasLinkList = true;
            }
        }

        public void SetFolderPath(string folderPath)
        {
            FolderPath = folderPath;
        }

        /// <summary>
        /// Load an ISOXML TaskSet and return an ISOXML Object
        /// </summary>
        /// <param name="path">The Path from where the data shall be loaded </param>
        /// <param name="loadBinData">Shall all binary data such as grids and TLGs be loaded? Default is true</param>
        /// <returns></returns>
        public static ISOXML Load(string path, bool loadBinData = true)
        {
            var resultTaskData = TaskData.LoadTaskData(path);
            var isoxml = new ISOXML(path)
            {
                Data = resultTaskData.Result,
                Messages = resultTaskData.Messages
            };

            if (isoxml.Data.AttachedFile != null)
            {
                foreach (var file in isoxml.Data.AttachedFile)
                {
                    if (file.FileType == 1 /*LinkList*/)
                    {
                        //The parameters of the AttachedFileObject are not used, we assume the file is called LinkList as defined in the standard!
                        var resultLinkList = IsoLinkList.LoadLinkList(path, file.FilenameWithExtension);
                        isoxml.LinkList = resultLinkList.Result;
                        isoxml.Messages.AddRange(resultLinkList.Messages);
                        isoxml.HasLinkList = true;
                        break;//TODO We currently only support one attached File!
                    }
                }
            }

            isoxml.ReadIDTable();

            if (loadBinData)
            {
                isoxml.LoadBinaryData();
            }
            isoxml.InitExtensionData();

            return isoxml;
        }

        /// <summary>
        /// Initialize all such elements that extend the pure ISOXML Functionality in the ISOExtensions Folder
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void InitExtensionData()
        {
            foreach (var task in Data.Task)
            {
                task.initTimeLogList(TimeLogs);
            }
        }

        public ISOGrid GenerateGrid(ISOGridType type, uint width, uint height, byte layers)
        {
            var grid = new ISOGrid()
            {
                GridMaximumColumn = width,
                GridMaximumRow = height,
                GridType = type

            };
            _maxGridIndex++;
            grid.Filename = ISOGridFile.GenerateName(_maxGridIndex);
            Grids.Add(grid.Filename, ISOGridFile.Create(grid, layers));

            return grid;
        }

        public ISOGridFile GetGridFile(ISOGrid iSOGrid)
        {
            return Grids[iSOGrid.Filename];
        }

        public int CountValidTimeLogs()
        {
            int counts = 0;
            foreach (var tlg in TimeLogs)
            {
                if (tlg.Value.Loaded == TLGStatus.LOADED)
                {
                    counts++;
                }
            }

            return counts;
        }


        /// <summary>
        /// This generates a new, empty ISOXML TaskSet
        /// </summary>
        /// <param name="outPath"></param>
        /// <returns></returns>
        public static ISOXML Create(string outPath)
        {
            return new ISOXML(outPath);
        }

        /// <summary>
        /// Loads the given FileSet asynchronously
        /// </summary>
        /// <param name="path"></param>
        /// <param name="loadBinData"></param>
        /// <returns></returns>
        public static async AsyncTask.Task<ISOXML> LoadAsync(string path, bool loadBinData = true)
        {
            return await AsyncTask.Task.Run(() => Load(path, loadBinData));
        }

        /// <summary>
        /// Iterates through the given TASKDATA.XML and fills the IDList Tables
        /// </summary>
        private void ReadIDTable()
        {
            foreach (var obj in Data.BaseStation)
            {
                IdTable.ReadObject(obj);
            }
            foreach (var obj in Data.CodedComment)
            {
                IdTable.ReadObject(obj);
            }
            foreach (var obj in Data.CodedCommentGroup)
            {
                IdTable.ReadObject(obj);
            }
            foreach (var obj in Data.CropType)
            {
                IdTable.ReadObject(obj);
            }
            foreach (var obj in Data.CulturalPractice)
            {
                IdTable.ReadObject(obj);
            }
            foreach (var obj in Data.Customer)
            {
                IdTable.ReadObject(obj);
            }
            foreach (var obj in Data.Device)
            {
                IdTable.ReadObject(obj);
            }
            foreach (var obj in Data.Farm)
            {
                IdTable.ReadObject(obj);
            }
            foreach (var obj in Data.OperationTechnique)
            {
                IdTable.ReadObject(obj);
            }
            foreach (var obj in Data.Partfield)
            {
                IdTable.ReadObject(obj);
            }
            foreach (var obj in Data.Product)
            {
                IdTable.ReadObject(obj);
            }
            foreach (var obj in Data.Task)
            {
                IdTable.ReadObject(obj);
            }
            foreach (var obj in Data.ValuePresentation)
            {
                IdTable.ReadObject(obj);
            }
            foreach (var obj in Data.Worker)
            {
                IdTable.ReadObject(obj);
            }
        }

        /// <summary>
        /// Reads the binary data if not yet done
        /// </summary>
        public void LoadBinaryData()
        {
            if (_binaryLoaded == false)
            {
                LoadGrids();
                LoadTimeLogs();
                _binaryLoaded = true;

            }

        }

        public AsyncTask.Task LoadBinaryDataAsync()
        {
            var waiter = AsyncTask.Task.Run(() => LoadBinaryData());
            return waiter;
        }

        private int LoadTimeLogs()
        {
            foreach (var task in Data.Task)
            {
                foreach (var tlg in task.TimeLog)
                {
                    var entry = ISOTLG.LoadTLG(tlg.Filename, FolderPath);
                    Messages.AddRange(entry.Messages);
                    TimeLogs.Add(tlg.Filename, entry.Result);
                }
            }
            return TimeLogs.Count;

        }

        private int LoadGrids()
        {
            if (Data == null)
            {
                return 0;
            }

            Grids = new Dictionary<string, ISOGridFile>();
            foreach (var task in Data.Task)
            {
                if (task.Grid != null && task.Grid.Count > 0)
                {
                    var grid = task.Grid[0];
                    byte layers = 0;
                    foreach (var tzn in task.TreatmentZone)
                    {
                        if (grid.TreatmentZoneCode == tzn.TreatmentZoneCode)
                        {
                            layers = (byte)tzn.ProcessDataVariable.Count;
                            break;
                        }
                    }
                    var index = uint.Parse(grid.Filename.Substring(3, 5));
                    if (index > _maxGridIndex)
                    {
                        _maxGridIndex = index;
                    }

                    var result = ISOGridFile.Load(FolderPath, grid.Filename, (uint)grid.GridMaximumColumn, (uint)grid.GridMaximumRow, grid.GridType, layers);
                    Grids.Add(task.Grid[0].Filename, result.Result);
                    Messages.AddRange(result.Messages);
                }
            }
            return Grids.Count;
        }

        /// <summary>
        /// This saves the ISOXML FileSet including all binary and attached files
        /// All TASKDATA.XML Elements are stored within the main file TASKDATA.XML, not in external CTR00001.XML Files
        /// </summary>
        public void Save()
        {
            if (VersionMajor != ISO11783TaskDataFileVersionMajor.Version4)
            {
                UpdateDataForV3();
            }

            TaskData.SaveTaskData(Data, FolderPath);
            if (HasLinkList)
            {
                LinkList.SaveLinkList(FolderPath);
            }
            foreach (var entry in Grids)
            {
                entry.Value.Save(Path.Combine(FolderPath, entry.Key + ".BIN"));
            }
        }

        private void UpdateDataForV3()
        {
            //change taskStatus template and Canceled
            foreach (var task in Data.Task)
            {
                switch (task.TaskStatus)
                {
                    //p.141
                    case ISOTaskStatus.Template:
                    case ISOTaskStatus.Canceled:
                        task.TaskStatus = ISOTaskStatus.Planned;
                        break;
                    default:
                        break;
                }

                if (task.GuidanceAllocationSpecified)
                {
                    //clear GuidanceAllocation and GuidanceShift
                    //p.142
                    task.GuidanceAllocation.Clear();
                }

                //Time doesn't have a timezone included in a V3. But it also don't have it in our implementation.
                foreach (var time in task.Time)
                {
                    if(time.Type == ISOType2.PoweredDown)
                    {
                        //p.146
                        time.Type = ISOType2.Clearing;
                    }

                }
                //described on p.75
                if (task.ControlAssignmentSpecified)
                {
                    //p.142
                    task.ControlAssignment.Clear();
                }

                if (task.TreatmentZoneSpecified)
                {
                    foreach (var trZone in task.TreatmentZone)
                    {
                        if(trZone.ProcessDataVariable.Count > 1)
                        {
                            //leave only one element
                            //p.148
                            trZone.ProcessDataVariable.ToList().RemoveRange(1, trZone.ProcessDataVariable.Count - 1);
                        }

                        //p.129
                        trZone.ProcessDataVariable.First().ActualCulturalPracticeValue = null;
                        trZone.ProcessDataVariable.First().ElementTypeInstanceValue = null;

                        if (trZone.PolygonTreatmentZoneonly.Count > 1)
                        {
                            trZone.PolygonTreatmentZoneonly.ToList().RemoveRange(1, trZone.PolygonTreatmentZoneonly.Count - 1);
                        }
                    }
                }

                foreach (var pAlloc in task.ProductAllocation)
                {
                    if(pAlloc.TransferMode.HasValue && pAlloc.TransferMode == ISOTransferMode.Remainder)
                    {
                        //p.136
                        pAlloc.TransferMode = ISOTransferMode.Emptying;
                    }
                }
            }

            foreach (var partfield in Data.Partfield)
            {
                if (partfield.GuidanceGroupSpecified)
                {
                    //clear GuidanceGroups and GuidancePatterns
                    partfield.GuidanceGroup.Clear();
                }

                foreach (var line in partfield.LineString)
                {
                    if (line.LineStringType == ISOLineStringType.Obstacle)
                    {
                        line.LineStringType = ISOLineStringType.Flag;//p.117
                        line.LineStringId = null; //p.118
                        foreach (var point in line.Point)
                        {
                            //REMARK DR: PointType 3; should become 1

                            //REMARK DR: Just delete all Points with a Type of abouve ISOPOintType.other.
                            if (point.PointType > ISOPointType.other)
                            {
                                point.PointType = ISOPointType.other; //p.123
                            }
                            //p.124
                            point.PointId = null;
                            point.PointHorizontalAccuracy = null;
                            point.PointVerticalAccuracy = null;
                            //REMARK DR: If we have a filename, we need to load the PointFile and convert it into Points. 
                            point.Filename = null;
                            point.Filelength = null;
                        }
                    }
                }

                foreach (var polygon in partfield.PolygonnonTreatmentZoneonly)
                {
                    //p.125
                    if(polygon.PolygonType > ISOPolygonType.Other)
                    {
                        polygon.PolygonType = ISOPolygonType.Other;
                    }
                    polygon.PolygonId = null;
                }
            }


            //REMARK FW: There is an inofficial definition on how to handle Product Mixtures in V3. Should be implemented here
            foreach (var product in Data.Product)
            {
                if (product.ProductRelationSpecified)
                {
                    //clear ProductRelation
                    product.ProductRelation.Clear();
                }
                //p.133
                product.ProductType = null;
                product.MixtureRecipeQuantity = null;
                product.DensityMassPerVolume = null;
                product.DensityMassPerCount = null;
                product.DensityVolumePerCount = null;
            }

            if (Data.TaskControllerCapabilitiesSpecified)
            {
                Data.TaskControllerCapabilities.Clear();

            }

            //remove attached files
            if (Data.AttachedFileSpecified)
            {
                Data.AttachedFile.Clear();

                //TODO: make sure it is correct way
                //REMARK: Don't just delete the structure; we need another way to store the LinkList File in our Own Database.
                LinkList = null;
                HasLinkList = false;
            }
            //Data.Device "Number of Extended Structure Label bytes" property is not presented in our xsd file (p.46 of the PDF)
            //REMARK DR: The StructureLabel is in DVC.F. The Extended StructueLabel just makes this one longer. While in Version 3 you must have 7 bytes, you can have 7-39 bytes in V4. In case of V3
            //          just cut at the beginning so that 7 bytes are left
            //REMARK FW: There is one ByteArray that switched its Order between V3 & V4 FW to check which ones those are

            //Peer control assignment messages (p.56) What is this?
            //REMARK DR: Peer control allows one machine to control another one.For example there is a nitrogen sensor in front of the tractor that directly controls the fertilizer in the rear of
            //           the Tractor. The relevant ISOXML element is CAT (Control Assignment), you've handled it all correct :-) 

            if (Data.BaseStationSpecified)
            {
                Data.BaseStation.Clear();
            }

            if (Data.CropTypeSpecified)
            {
                foreach (var crop in Data.CropType)
                {
                    crop.ProductGroupIdRef = null; //p.87
                    if (crop.CropVarietySpecified)
                    {
                        foreach (var item in crop.CropVariety)
                        {
                            item.ProductIdRef = null; //p.88
                        }
                    }
                }
            }

            if (Data.DeviceSpecified)
            {
                foreach (var device in Data.Device)
                {
                    foreach (var item in device.DeviceProcessData)
                    {
                        if (item.DeviceProcessDataProperty == 4)//REMARK DR: This is BitEncoded, so we need to check if the BIT representing 4 is set, not if the Value is 4. you can just unset the bit representing 4
                        {
                            item.DeviceProcessDataProperty = 1; // p.99
                        }
                    }
                }
            }

            Data.lang = null; //p.115
            if (Data.TaskControllerCapabilitiesSpecified)
            {
                Data.TaskControllerCapabilities.Clear();//p.116, p.143
            }

            if (Data.ProductGroupSpecified)
            {
                foreach (var productGroup in Data.ProductGroup)
                {
                    //p.139
                    productGroup.ProductGroupType = null;
                }
            }
        }

        public AsyncTask.Task SaveAsync()
        {
            return AsyncTask.Task.Run(() => Save());
        }
    }
}
