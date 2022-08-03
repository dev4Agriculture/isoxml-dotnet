using Dev4ag.ISO11783.TaskFile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static Dev4ag.TaskData;

namespace Dev4ag
{
    public class IsoGrid
    {

        public UInt32 width;
        public UInt32 height;
        public Byte layers;//Only for Type2
        public byte[,] datat1;
        public UInt32[,,] datat2;
        public string name;
        public GridType type;
        public bool loaded;



        public static ResultWithMessages<IsoGrid> Load(String baseFolder, string name, UInt32 width, UInt32 height, GridType type,Byte layers)
        {
            List<ResultMessage> messages = new List<ResultMessage>();
            IsoGrid grid = null;
            string filePath = Path.Combine(baseFolder, name + ".BIN");
            if (File.Exists(filePath))
            {
                grid = new IsoGrid()
                {
                    width = width,
                    height = height,
                    type = type,
                    name = name,
                };
                FileStream binaryFileStream = File.Open(filePath, FileMode.Open);
                switch (grid.type)
                {
                    case GridType.gridtype1:
                        grid.datat1 = new byte[grid.height, grid.width];
                        if (grid.width * grid.height == binaryFileStream.Length)
                        {

                            byte[] buffer = new byte[grid.width];
                            for(int y = 0; y< grid.height; y++) {
                                int readDataLength = binaryFileStream.Read(buffer, 0, buffer.Length);
                                if(readDataLength == grid.width) {
                                    for(int x = 0; x < grid.width; x++)
                                    {
                                        grid.datat1[y, x] = buffer[x];
                                    }
                                }
                            }
                        } else
                        {
                            messages.Add(new ResultMessage(ResultMessageType.Error, "FileSize of Grid doesn't match: " + filePath));
                        }
                        break;
                    case GridType.gridtype2:
                        grid.datat2 = new UInt32[grid.layers,grid.height, grid.width];
                        if (grid.layers * grid.width * grid.height * sizeof(UInt32)== binaryFileStream.Length)
                        {

                            for( int l = 0; l< layers; l++)
                            {
                                byte[] buffer = new byte[grid.width * sizeof(UInt32)];
                                for (int y = 0; y < grid.height; y++)
                                {
                                    int readDataLength = binaryFileStream.Read(buffer, 0, buffer.Length);
                                    if (readDataLength == buffer.Length)
                                    {
                                        for (int x = 0; x < grid.width; x++)
                                        {
                                            grid.datat2[l,y, x] = BitConverter.ToUInt32(buffer, x * 4);
                                        }
                                    }
                                }

                            }
                        }
                        else
                        {
                            messages.Add(new ResultMessage(ResultMessageType.Error, "FileSize of Grid doesn't match: " + filePath));
                        }

                        break;
                }

                binaryFileStream.Close();
                grid.loaded = true;
            } else
            {
                messages.Add(new ResultMessage(ResultMessageType.Error, "Could not find Grid File: " + filePath));
            }
            return new ResultWithMessages<IsoGrid>(grid)
            {
                messages = messages
            };
        }

        public void save(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fs);
            switch (this.type)
            {
                case GridType.gridtype1:
                    for (int y = 0; y < this.height; y++)
                    {
                        for (int x = 0; x < this.width; x++)
                        {
                            bw.Write(this.datat1[y, x]);
                        }
                    }

                    break;
                case GridType.gridtype2:
                    for( int l = 0; l< layers; l++)
                    {
                        for (int y = 0; y < this.height; y++)
                        {
                            for (int x = 0; x < this.width; x++)
                            {
                                bw.Write(this.datat2[l,y, x]);
                            }
                        }
                    }
                    break;
            }
            bw.Close();
            fs.Close();
        }


        public void saveCSV(string storagePath)
        {
            try
            {
                string filePath = storagePath + this.name + ".CSV";
                FileStream file = File.Create(filePath);
                StreamWriter streamWriter = new StreamWriter(file);
                switch (this.type)
                {
                    case GridType.gridtype1:
                        for (int y = 0; y < this.height; y++)
                        {
                            string dataLineText = "";
                            for (int x = 0; x < this.width; x++)
                            {
                                dataLineText = dataLineText + this.datat1[this.height - 1 - y, x] + ";";
                            }
                            streamWriter.WriteLine(dataLineText);
                            streamWriter.Flush();
                        }
                        break;
                    case GridType.gridtype2:
                        for(int l=0; l < this.layers; l++)
                        {
                            for (int y = 0; y < this.height; y++)
                            {
                                string dataLineText = "";
                                for (int x = 0; x < this.width; x++)
                                {
                                    dataLineText = dataLineText + this.datat2[l,this.height - 1 - y, x] + ";";
                                }
                                streamWriter.WriteLine(dataLineText);
                                streamWriter.Flush();
                            }
                        }
                        break;

                }

                streamWriter.Close();
                file.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not open file " + storagePath + "; canceling to write:"+ e.ToString());
            }
        }
    }
}
