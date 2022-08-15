using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static Dev4Agriculture.ISO11783.ISOXML.TaskData;

namespace Dev4Agriculture.ISO11783.ISOXML
{
    public class IsoGrid
    {

        public UInt32 width;
        public UInt32 height;
        public Byte layers;//Only for Type2
        private byte[,] datat1;
        private UInt32[,,] datat2;
        public string name;
        public ISOGridType type;
        public bool loaded;



        public static ResultWithMessages<IsoGrid> Load(String baseFolder, string name, UInt32 width, UInt32 height, ISOGridType type,Byte layers)
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
                    layers = layers
                };
                FileStream binaryFileStream = File.Open(filePath, FileMode.Open);
                switch (grid.type)
                {
                    case ISOGridType.gridtype1:
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
                    case ISOGridType.gridtype2:
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
                case ISOGridType.gridtype1:
                    for (int y = 0; y < this.height; y++)
                    {
                        for (int x = 0; x < this.width; x++)
                        {
                            bw.Write(this.datat1[y, x]);
                        }
                    }

                    break;
                case ISOGridType.gridtype2:
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
                    case ISOGridType.gridtype1:
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
                    case ISOGridType.gridtype2:
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

        public int setValue(uint x, uint y, uint value, uint layer = 0)
        {
            if ((x < 0 || x >= width) || (y<0 || y >= height)){
                return 0;
            }

            switch (this.type)
            {
                case ISOGridType.gridtype1:
                    if(value<0  ||value> 255) {
                        return 0;
                    }
                    this.datat1[y, x] = (byte)value;
                    return 1;
                case ISOGridType.gridtype2:
                    if(layer<0 || layer>= layers)
                    {
                        return 0;
                    }
                    this.datat2[layer,y,x] = value;
                    return 1;
                default:
                    return 0;
            }
        }

        public uint getValue(uint x, uint y, uint layer)
        {
            if ((x < 0 || x >= width) || (y < 0 || y >= height))
            {
                throw new IndexOutOfRangeException();
            }

            switch (this.type)
            {
                case ISOGridType.gridtype1:
                    return this.datat1[y, x];
                case ISOGridType.gridtype2:
                    if (layer < 0 || layer >= layers)
                    {
                        throw new IndexOutOfRangeException();
                    }
                    return this.datat2[layer, y, x];
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}
