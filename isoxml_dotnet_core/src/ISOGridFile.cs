using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using System;
using System.Collections.Generic;
using System.IO;

namespace Dev4Agriculture.ISO11783.ISOXML
{
    public class ISOGridFile
    {

        public UInt32 Width;
        public UInt32 Height;
        public Byte Layers;//Only for Type2
        private byte[,] Datat1;
        private UInt32[,,] Datat2;
        public string Name;
        public ISOGridType Type;
        public bool Loaded;

        private ISOGridFile(ISOGridType type)
        {
            Datat1 = new byte[1, 1];
            Datat2 = new uint[1, 1, 1];
            Width = 1;
            Height = 1;
            Layers = 1;
            Type = type;
        }

        public static ResultWithMessages<ISOGridFile> Load(String baseFolder, string name, UInt32 width, UInt32 height, ISOGridType type, Byte layers)
        {
            List<ResultMessage> messages = new List<ResultMessage>();
            ISOGridFile grid = null;
            string filePath = Path.Combine(baseFolder, name + ".BIN");
            if (File.Exists(filePath))
            {
                grid = new ISOGridFile(type);

                grid.Init(width, height, layers);
                FileStream binaryFileStream = File.Open(filePath, FileMode.Open);
                switch (grid.Type)
                {
                    case ISOGridType.gridtype1:
                        grid.Datat1 = new byte[grid.Height, grid.Width];
                        if (grid.Width * grid.Height == binaryFileStream.Length)
                        {

                            byte[] buffer = new byte[grid.Width];
                            for (int y = 0; y < grid.Height; y++)
                            {
                                int readDataLength = binaryFileStream.Read(buffer, 0, buffer.Length);
                                if (readDataLength == grid.Width)
                                {
                                    for (int x = 0; x < grid.Width; x++)
                                    {
                                        grid.Datat1[y, x] = buffer[x];
                                    }
                                }
                            }
                        }
                        else
                        {
                            messages.Add(new ResultMessage(ResultMessageType.Error, "FileSize of Grid doesn't match: " + filePath));
                        }
                        break;
                    case ISOGridType.gridtype2:
                        grid.Datat2 = new UInt32[grid.Layers, grid.Height, grid.Width];
                        if (grid.Layers * grid.Width * grid.Height * sizeof(UInt32) == binaryFileStream.Length)
                        {

                            for (int l = 0; l < layers; l++)
                            {
                                byte[] buffer = new byte[grid.Width * sizeof(UInt32)];
                                for (int y = 0; y < grid.Height; y++)
                                {
                                    int readDataLength = binaryFileStream.Read(buffer, 0, buffer.Length);
                                    if (readDataLength == buffer.Length)
                                    {
                                        for (int x = 0; x < grid.Width; x++)
                                        {
                                            grid.Datat2[l, y, x] = BitConverter.ToUInt32(buffer, x * 4);
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
                grid.Loaded = true;
            }
            else
            {
                messages.Add(new ResultMessage(ResultMessageType.Error, "Could not find Grid File: " + filePath));
            }
            return new ResultWithMessages<ISOGridFile>(grid)
            {
                messages = messages
            };
        }

        internal static string GenerateName(uint gridIndex)
        {
            return "GRD" + gridIndex.ToString().PadLeft(5, '0');
        }

        public static ISOGridFile Create(ISOGrid grid, byte layers = 1)
        {
            var gridFile = new ISOGridFile(grid.GridType);

            gridFile.Init((uint)grid.GridMaximumColumn, (uint)grid.GridMaximumRow, layers);

            return gridFile;
        }

        public void Save(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fs);
            switch (this.Type)
            {
                case ISOGridType.gridtype1:
                    for (int y = 0; y < this.Height; y++)
                    {
                        for (int x = 0; x < this.Width; x++)
                        {
                            bw.Write(this.Datat1[y, x]);
                        }
                    }

                    break;
                case ISOGridType.gridtype2:
                    for (int l = 0; l < Layers; l++)
                    {
                        for (int y = 0; y < this.Height; y++)
                        {
                            for (int x = 0; x < this.Width; x++)
                            {
                                bw.Write(this.Datat2[l, y, x]);
                            }
                        }
                    }
                    break;
            }
            bw.Close();
            fs.Close();
        }


        public void SaveCSV(string storagePath)
        {
            try
            {
                string filePath = storagePath + this.Name + ".CSV";
                FileStream file = File.Create(filePath);
                StreamWriter streamWriter = new StreamWriter(file);
                switch (this.Type)
                {
                    case ISOGridType.gridtype1:
                        for (int y = 0; y < this.Height; y++)
                        {
                            string dataLineText = "";
                            for (int x = 0; x < this.Width; x++)
                            {
                                dataLineText = dataLineText + this.Datat1[this.Height - 1 - y, x] + ";";
                            }
                            streamWriter.WriteLine(dataLineText);
                            streamWriter.Flush();
                        }
                        break;
                    case ISOGridType.gridtype2:
                        for (int l = 0; l < this.Layers; l++)
                        {
                            for (int y = 0; y < this.Height; y++)
                            {
                                string dataLineText = "";
                                for (int x = 0; x < this.Width; x++)
                                {
                                    dataLineText = dataLineText + this.Datat2[l, this.Height - 1 - y, x] + ";";
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
                Console.WriteLine("Could not open file " + storagePath + "; canceling to write:" + e.ToString());
                throw e;//TODO: Not the most beautiful way to handle errors here
            }
        }

        public void Init(uint width, uint height, byte layers)
        {
            this.Width = width;
            this.Height = height;
            this.Layers = layers;
            switch (this.Type)
            {
                case ISOGridType.gridtype1:
                    this.Datat1 = new byte[height, width];
                    break;
                case ISOGridType.gridtype2:
                    this.Datat2 = new uint[layers, height, width];
                    break;
            }
        }

        public int SetValue(uint column, uint row, uint value, uint layer = 0)
        {
            if ((column < 0 || column >= Width) || (row < 0 || row >= Height))
            {
                return 0;
            }

            switch (this.Type)
            {
                case ISOGridType.gridtype1:
                    if (value < 0 || value > 255)
                    {
                        return 0;
                    }
                    this.Datat1[row, column] = (byte)value;
                    return 1;
                case ISOGridType.gridtype2:
                    if (layer < 0 || layer >= Layers)
                    {
                        return 0;
                    }
                    this.Datat2[layer, row, column] = value;
                    return 1;
                default:
                    return 0;
            }
        }

        public uint GetValue(uint column, uint row, uint layer)
        {
            if ((column < 0 || column >= Width) || (row < 0 || row >= Height))
            {
                throw new IndexOutOfRangeException();
            }

            switch (this.Type)
            {
                case ISOGridType.gridtype1:
                    return this.Datat1[row, column];
                case ISOGridType.gridtype2:
                    if (layer < 0 || layer >= Layers)
                    {
                        throw new IndexOutOfRangeException();
                    }
                    return this.Datat2[layer, row, column];
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}
