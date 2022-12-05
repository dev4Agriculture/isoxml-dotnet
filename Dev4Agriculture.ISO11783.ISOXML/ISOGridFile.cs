using System;
using System.Collections.Generic;
using System.IO;
using Dev4Agriculture.ISO11783.ISOXML.Messaging;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;

namespace Dev4Agriculture.ISO11783.ISOXML
{
    public class ISOGridFile
    {

        public uint Width { get; private set; }
        public uint Height { get; private set; }
        public byte Layers { get; private set; }
        private byte[,] _datat1;
        private uint[,,] _datat2;
        public string Name;
        public ISOGridType Type { get; private set; }
        public bool Loaded { get; private set; }

        private ISOGridFile(ISOGridType type)
        {
            _datat1 = new byte[1, 1];
            _datat2 = new uint[1, 1, 1];
            Width = 1;
            Height = 1;
            Layers = 1;
            Type = type;
        }


        /// <summary>
        /// Load a Grid from a folder. This is normally directly called in ISOXML.Load;
        /// the function is only public for edge cases.
        /// </summary>
        /// <param name="baseFolder"></param>
        /// <param name="name"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="type"></param>
        /// <param name="layers"></param>
        /// <returns></returns>
        public static ResultWithMessages<ISOGridFile> Load(string baseFolder, string name, uint width, uint height, ISOGridType type, byte layers)
        {
            var messages = new List<ResultMessage>();
            ISOGridFile grid = null;
            if (Utils.AdjustFileNameToIgnoreCasing(baseFolder, name + ".BIN", out var filePath))
            {
                grid = new ISOGridFile(type);

                grid.Init(width, height, layers);
                var binaryFileStream = File.Open(filePath, FileMode.Open);
                switch (grid.Type)
                {
                    case ISOGridType.gridtype1:
                        grid._datat1 = new byte[grid.Height, grid.Width];
                        if (grid.Width * grid.Height == binaryFileStream.Length)
                        {

                            var buffer = new byte[grid.Width];
                            for (var y = 0; y < grid.Height; y++)
                            {
                                var readDataLength = binaryFileStream.Read(buffer, 0, buffer.Length);
                                if (readDataLength == grid.Width)
                                {
                                    for (var x = 0; x < grid.Width; x++)
                                    {
                                        grid._datat1[y, x] = buffer[x];
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
                        grid._datat2 = new uint[grid.Layers, grid.Height, grid.Width];
                        if (grid.Layers * grid.Width * grid.Height * sizeof(uint) == binaryFileStream.Length)
                        {

                            for (var l = 0; l < layers; l++)
                            {
                                var buffer = new byte[grid.Width * sizeof(uint)];
                                for (var y = 0; y < grid.Height; y++)
                                {
                                    var readDataLength = binaryFileStream.Read(buffer, 0, buffer.Length);
                                    if (readDataLength == buffer.Length)
                                    {
                                        for (var x = 0; x < grid.Width; x++)
                                        {
                                            grid._datat2[l, y, x] = BitConverter.ToUInt32(buffer, x * 4);
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
                Messages = messages
            };
        }

        internal static string GenerateName(uint gridIndex)
        {
            return "GRD" + gridIndex.ToString().PadLeft(5, '0');
        }

        /// <summary>
        /// Create a new GridFile from an ISOGrid-Structure.
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="layers"></param>
        /// <returns></returns>
        public static ISOGridFile Create(ISOGrid grid, byte layers = 1)
        {
            var gridFile = new ISOGridFile(grid.GridType);

            gridFile.Init((uint)grid.GridMaximumColumn, (uint)grid.GridMaximumRow, layers);

            return gridFile;
        }

        /// <summary>
        /// Stores the given Grid into the given Path. The Path shall be the filePath including the full filename.
        /// e.g. "C://data/GRD00001.bin
        /// </summary>
        /// <param name="path"></param>
        public void Save(string path)
        {
            var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            var bw = new BinaryWriter(fs);
            switch (Type)
            {
                case ISOGridType.gridtype1:
                    for (var y = 0; y < Height; y++)
                    {
                        for (var x = 0; x < Width; x++)
                        {
                            bw.Write(_datat1[y, x]);
                        }
                    }

                    break;
                case ISOGridType.gridtype2:
                    for (var l = 0; l < Layers; l++)
                    {
                        for (var y = 0; y < Height; y++)
                        {
                            for (var x = 0; x < Width; x++)
                            {
                                bw.Write(_datat2[l, y, x]);
                            }
                        }
                    }
                    break;
            }
            bw.Close();
            fs.Close();
        }

        /// <summary>
        /// Exports the Grid to a CSV file; Column-Delimiter is ";"
        /// </summary>
        /// <param name="storagePath"></param>
        public void SaveCSV(string storagePath)
        {
            try
            {
                var filePath = storagePath + Name + ".CSV";
                var file = File.Create(filePath);
                var streamWriter = new StreamWriter(file);
                switch (Type)
                {
                    case ISOGridType.gridtype1:
                        for (var y = 0; y < Height; y++)
                        {
                            var dataLineText = "";
                            for (var x = 0; x < Width; x++)
                            {
                                dataLineText = dataLineText + _datat1[Height - 1 - y, x] + ";";
                            }
                            streamWriter.WriteLine(dataLineText);
                            streamWriter.Flush();
                        }
                        break;
                    case ISOGridType.gridtype2:
                        for (var l = 0; l < Layers; l++)
                        {
                            for (var y = 0; y < Height; y++)
                            {
                                var dataLineText = "";
                                for (var x = 0; x < Width; x++)
                                {
                                    dataLineText = dataLineText + _datat2[l, Height - 1 - y, x] + ";";
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

        /// <summary>
        /// Init or reinit a Grid to set its 3 dimensions
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="layers"></param>
        public void Init(uint width, uint height, byte layers)
        {
            Width = width;
            Height = height;
            Layers = layers;
            switch (Type)
            {
                case ISOGridType.gridtype1:
                    _datat1 = new byte[height, width];
                    break;
                case ISOGridType.gridtype2:
                    _datat2 = new uint[layers, height, width];
                    break;
            }
        }


        /// <summary>
        /// Set the value at a specific field of the Grid.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <param name="value"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public int SetValue(uint column, uint row, uint value, uint layer = 0)
        {
            if (column < 0 || column >= Width || row < 0 || row >= Height)
            {
                return 0;
            }

            switch (Type)
            {
                case ISOGridType.gridtype1:
                    if (value < 0 || value > 255)
                    {
                        return 0;
                    }
                    _datat1[row, column] = (byte)value;
                    return 1;
                case ISOGridType.gridtype2:
                    if (layer < 0 || layer >= Layers)
                    {
                        return 0;
                    }
                    _datat2[layer, row, column] = value;
                    return 1;
                default:
                    return 0;
            }
        }


        /// <summary>
        /// Read a value from a specific cell in the Grid
        /// </summary>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public uint GetValue(uint column, uint row, uint layer)
        {
            if (column < 0 || column >= Width || row < 0 || row >= Height)
            {
                throw new IndexOutOfRangeException();
            }

            switch (Type)
            {
                case ISOGridType.gridtype1:
                    return _datat1[row, column];
                case ISOGridType.gridtype2:
                    if (layer < 0 || layer >= Layers)
                    {
                        throw new IndexOutOfRangeException();
                    }
                    return _datat2[layer, row, column];
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}
