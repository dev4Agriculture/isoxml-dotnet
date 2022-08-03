using Dev4ag.ISO11783.TaskFile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dev4ag
{
    public class ISOXML
    {
        public Dictionary<IsoGrid, Task> grids;
        public ISO11783TaskDataFile data;
        public string folderPath;
        public List<ResultMessage> messages;
        public IDTable idTable;

        public ISOXML(string path)
        {
            data = new ISO11783TaskDataFile();
            grids = new Dictionary<IsoGrid, Task>();
            folderPath = path;
            messages = new List<ResultMessage>(); 
            idTable = new IDTable();
        }


        public static ISOXML Load(string path, bool loadBinData = true)
        {
            var result = TaskData.LoadTaskData(path);
            var isoxml = new ISOXML(path)
            {
                data = result.result,
                messages = result.messages
            };

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

            grids = new Dictionary<IsoGrid, Task>();
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
                    grids.Add(result.result, task);
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
            foreach(var grid in this.grids)
            {
                grid.Key.save(Path.Combine(this.folderPath, grid.Value.Grid[0].Filename + ".BIN"));
            }
        }



    }
}
