using Dev4ag.ISO11783.LinkListFile;
using Dev4ag.ISO11783.TaskFile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dev4ag
{
    public class ISOXML
    {
        public Dictionary<string,IsoGrid> grids;
        public ISO11783TaskDataFile data;
        public string folderPath;
        public List<ResultMessage> messages;
        public IDTable idTable;

        public ISO11783LinkListFile links;

        public ISOXML(string path)
        {
            data = new ISO11783TaskDataFile();
            grids = new Dictionary<string, IsoGrid>();
            folderPath = path;
            messages = new List<ResultMessage>(); 
            idTable = new IDTable();
            links = new ISO11783LinkListFile();
        }


        public static ISOXML Load(string path, bool loadBinData = true)
        {
            var resultTaskData = TaskData.LoadTaskData(path);
            var resultLinkList = IsoLinkList.LoadLinkList(path);
            var isoxml = new ISOXML(path)
            {
                data = resultTaskData.result,
                links = resultLinkList.result
                
            };
            isoxml.messages.AddRange(resultTaskData.messages);
            isoxml.messages.AddRange(resultLinkList.messages);

            if (loadBinData)
            {
                isoxml.loadBinaryData();
            }

            return isoxml;
        }

        public void loadBinaryData()
        {
            loadGrids();

        }
        private int loadGrids() {
            if(data == null)
            {
                return 0;
            }

            grids = new Dictionary<string,IsoGrid>();
            foreach (var task in data.Task)
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
                    var result = IsoGrid.Load(this.folderPath,grid.Filename, (uint)grid.GridMaximumColumn, (uint)grid.GridMaximumRow,grid.GridType,layers);
                    grids.Add(task.Grid[0].Filename, result.result);
                    messages.AddRange(result.messages);
                }
            }
            return this.grids.Count;
        }

        internal void loadData(List<ResultMessage> messages)
        {
            throw new NotImplementedException();
        }

        public void save()
        {
            TaskData.SaveTaskData(this.data, this.folderPath);
            foreach(var entry in this.grids)
            {
                entry.Value.save(Path.Combine(this.folderPath, entry.Key + ".BIN"));
            }
        }



    }
}
