using Dev4Agriculture.ISO11783.ISOXML.Messaging;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using System;
using System.Collections.Generic;
using System.IO;

//Alias Definitions

using AsyncTask = System.Threading.Tasks;//Used for Task as this is a duplicate word with ISOXML Task.
namespace Dev4Agriculture.ISO11783.ISOXML
{
    public class ISOXML
    {
        public Dictionary<string, ISOGridFile> Grids { get; private set; }
        private uint _maxGridIndex = 0;
        public ISO11783TaskDataFile Data { get; private set; }
        public string FolderPath { get; private set; }
        public List<ResultMessage> Messages { get; private set; }
        public IdTable IdTable { get; private set; }
        public IsoLinkList LinkList { get; private set; }
        public bool HasLinkList { get; private set; }
        private bool _binaryLoaded;

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
            if (HasLinkList == false)
            {
                LinkList = new IsoLinkList();
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
                Data = resultTaskData.result,
                Messages = resultTaskData.messages
            };

            if (isoxml.Data.AttachedFile != null)
            {
                foreach (var file in isoxml.Data.AttachedFile)
                {
                    if (file.FileType == 1 /*LinkList*/)
                    {
                        //REMARK: The parameters of the AttachedFileObject are not used, we assume the file is called LinkList as defined in the standard!
                        var resultLinkList = IsoLinkList.LoadLinkList(path);
                        isoxml.LinkList = resultLinkList.result;
                        isoxml.Messages.AddRange(resultLinkList.messages);
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

            return isoxml;
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
            return await AsyncTask.Task<ISOXML>.Run(() => Load(path, loadBinData));
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
                _binaryLoaded = true;

            }

        }

        public AsyncTask.Task LoadBinaryDataAsync()
        {
            var waiter = AsyncTask.Task.Run(() => LoadBinaryData());
            return waiter;
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
                    Grids.Add(task.Grid[0].Filename, result.result);
                    Messages.AddRange(result.messages);
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
            TaskData.SaveTaskData(Data, FolderPath);
            if (HasLinkList == true)
            {
                LinkList.SaveLinkList(FolderPath);
            }
            foreach (var entry in Grids)
            {
                entry.Value.Save(Path.Combine(FolderPath, entry.Key + ".BIN"));
            }
        }

        public AsyncTask.Task SaveAsync()
        {
            return AsyncTask.Task.Run(() => Save());
        }



    }
}
