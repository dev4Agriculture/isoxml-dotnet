using Dev4ag.ISO11783.LinkListFile;
using Dev4ag.ISO11783.TaskFile;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

//Alias Definitions

using AsyncTask = System.Threading.Tasks;//Used for Task as this is a duplicate word with ISOXML Task.
namespace Dev4ag
{
    public class ISOXML
    {
        public Dictionary<string,IsoGrid> grids;
        public ISO11783TaskDataFile Data { get; private set; }
        public string FolderPath { get; private set; }
        public List<ResultMessage> Messages { get; private set; }
        public IdTable IdTable { get; private set; }
        public IsoLinkList LinkList { get; private set; }
        public bool HasLinkList { get; private set; }
        private bool binaryLoaded;

        /// <summary>
        ///  This generates an initial ISOXML Element. It does NOT Load any file
        /// </summary>
        /// <param name="path">The path where the ISOXML TaskSet should be stored</param>
        public ISOXML(string path)
        {
            Data = new ISO11783TaskDataFile();
            Data.ManagementSoftwareManufacturer = "unknown";
            Data.ManagementSoftwareVersion = "unknown"; 
            Data.TaskControllerManufacturer = "unknown";    
            Data.TaskControllerVersion = "unknown";
            Data.DataTransferOrigin = ISO11783TaskDataFileDataTransferOrigin.FMIS;
            grids = new Dictionary<string, IsoGrid>();
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
            if( HasLinkList == false)
            {
                LinkList = new IsoLinkList();
                Data.AttachedFile.Add(new AttachedFile()
                {
                    FileType = 1,
                    FilenameWithExtension = "LINKLIST.XML",
                    ManufacturerGLN = ""
                });
                HasLinkList=true;
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
            
            if( isoxml.Data.AttachedFile != null)
            {
                foreach(var file in isoxml.Data.AttachedFile)
                {
                    if( file.FileType == 1 /*LinkList*/)
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

        public static async AsyncTask.Task<ISOXML> LoadAsync(string path, bool loadBinData = true)
        {
            return await AsyncTask.Task<ISOXML>.Run(()=>Load(path, loadBinData));
        }

        private void ReadIDTable()
        {
            foreach(var obj in Data.BaseStation)
            {
                this.IdTable.ReadObject(obj);
            }
            foreach (var obj in Data.CodedComment)
            {
                this.IdTable.ReadObject(obj);
            }
            foreach (var obj in Data.CodedCommentGroup)
            {
                this.IdTable.ReadObject(obj);
            }
            foreach (var obj in Data.CropType)
            {
                this.IdTable.ReadObject(obj);
            }
            foreach (var obj in Data.CulturalPractice)
            {
                this.IdTable.ReadObject(obj);
            }
            foreach (var obj in Data.Customer)
            {
                this.IdTable.ReadObject(obj);
            }
            foreach (var obj in Data.Device)
            {
                this.IdTable.ReadObject(obj);
            }
            foreach (var obj in Data.Farm)
            {
                this.IdTable.ReadObject(obj);
            }
            foreach (var obj in Data.OperationTechnique)
            {
                this.IdTable.ReadObject(obj);
            }
            foreach (var obj in Data.Partfield)
            {
                this.IdTable.ReadObject(obj);
            }
            foreach (var obj in Data.Product)
            {
                this.IdTable.ReadObject(obj);
            }
            foreach (var obj in Data.Task)
            {
                this.IdTable.ReadObject(obj);
            }
            foreach (var obj in Data.ValuePresentation)
            {
                this.IdTable.ReadObject(obj);
            }
            foreach (var obj in Data.Worker)
            {
                this.IdTable.ReadObject(obj);
            }
        }

        public void LoadBinaryData()
        {
            if( this.binaryLoaded == false)
            {
                LoadGrids();
                this.binaryLoaded = true;

            }

        }
        private int LoadGrids() {
            if(Data == null)
            {
                return 0;
            }

            grids = new Dictionary<string,IsoGrid>();
            foreach (var task in Data.Task)
            {
                if(task.Grid != null && task.Grid.Count > 0)
                {
                    var grid = task.Grid[0];
                    Byte layers = 0;
                    foreach(var tzn in task.TreatmentZone)
                    {
                        if( grid.TreatmentZoneCode == tzn.TreatmentZoneCode)
                        {
                            layers = (Byte)tzn.ProcessDataVariable.Count;
                            break;
                        }
                    }
                    var result = IsoGrid.Load(this.FolderPath,grid.Filename, (uint)grid.GridMaximumColumn, (uint)grid.GridMaximumRow,grid.GridType,layers);
                    grids.Add(task.Grid[0].Filename, result.result);
                    Messages.AddRange(result.messages);
                }
            }
            return this.grids.Count;
        }


        /// <summary>
        /// This saves the ISOXML FileSet including all binary and attached files
        /// All TASKDATA.XML Elements are stored within the main file TASKDATA.XML, not in external CTR00001.XML Files
        /// </summary>
        public void Save()
        {
            TaskData.SaveTaskData(this.Data, this.FolderPath);
            if( this.HasLinkList == true)
            {
                this.LinkList.SaveLinkList(this.FolderPath);
            }
            foreach(var entry in this.grids)
            {
                entry.Value.save(Path.Combine(this.FolderPath, entry.Key + ".BIN"));
            }
        }

        public AsyncTask.Task SaveAsync()
        {
            return AsyncTask.Task.Run(() => Save());
        }



    }
}