using System;
using System.IO;
using Dev4Agriculture.ISO11783.ISOXML.Messaging;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.Utils;

namespace Dev4Agriculture.ISO11783.ISOXML
{
    public class ISOGridFile
    {

        public uint Width { get; private set; }
        public uint Height { get; private set; }
        public byte Layers { get; private set; }
        private byte[,,] _datat1;
        private int[,,] _datat2;
        public string Name;
        public ISOGridType Type { get; private set; }
        public bool Loaded { get; private set; }

        private ISOGridFile(ISOGridType type)
        {
            _datat1 = new byte[1, 1, 1];
            _datat2 = new int[1, 1, 1];
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
            var result = new ResultWithMessages<ISOGridFile>();
            ISOGridFile grid = null;
            var fileName = name + ".bin";
            if (FileUtils.HasMultipleFilesEndingWithThatName(baseFolder, fileName))
            {
                result.AddWarning(ResultMessageCode.FileNameEndingMultipleTimes, ResultDetail.FromString(fileName));
            }
            if (FileUtils.AdjustFileNameToIgnoreCasing(baseFolder, fileName, out var filePath))
            {
                grid = new ISOGridFile(type)
                {
                    Name = name
                };
                grid.Init(width, height, layers);
                var binaryFileStream = File.Open(filePath, FileMode.Open);
                switch (grid.Type)
                {
                    case ISOGridType.gridtype1:
                        grid._datat1 = new byte[grid.Height, grid.Width, grid.Layers];
                        if (grid.Width * grid.Height == binaryFileStream.Length)
                        {

                            for (var y = 0; y < grid.Height; y++)
                            {
                                var buffer = new byte[grid.Width * grid.Layers];
                                var readDataLength = binaryFileStream.Read(buffer, 0, buffer.Length);
                                if (readDataLength == grid.Width)
                                {
                                    for (var x = 0; x < grid.Width; x++)
                                    {
                                        for (var l = 0; l < grid.Layers; l++)
                                        {
                                            grid._datat1[y, x, l] = buffer[x * grid.Layers + l];
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            result.AddError(ResultMessageCode.GRIDFileSizeMissmatch, ResultDetail.FromPath(filePath));
                        }
                        break;
                    case ISOGridType.gridtype2:
                        grid._datat2 = new int[grid.Height, grid.Width, grid.Layers];
                        if (grid.Layers * grid.Width * grid.Height * sizeof(uint) == binaryFileStream.Length)
                        {
                            for (var y = 0; y < grid.Height; y++)
                            {
                                var buffer = new byte[grid.Width * grid.Layers * sizeof(uint)];
                                var readDataLength = binaryFileStream.Read(buffer, 0, buffer.Length);
                                if (readDataLength == buffer.Length)
                                {
                                    for (var x = 0; x < grid.Width; x++)
                                    {
                                        for (var l = 0; l < layers; l++)
                                        {
                                            grid._datat2[y, x, l] = BitConverter.ToInt32(buffer, (x * grid.Layers + l) * 4);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            result.AddError(
                                ResultMessageCode.GRIDFileSizeMissmatch,
                                ResultDetail.FromPath(filePath)
                                );
                        }

                        break;
                }

                binaryFileStream.Close();
                grid.Loaded = true;
            }
            else
            {
                result.AddError(
                    ResultMessageCode.FileNotFound,
                    ResultDetail.FromPath(filePath)
                    );
            }
            result.SetResult(grid);
            return result;
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
                            for (var l = 0; l < Layers; l++)
                            {
                                bw.Write(_datat1[y, x, l]);
                            }
                        }
                    }

                    break;
                case ISOGridType.gridtype2:
                    for (var y = 0; y < Height; y++)
                    {
                        for (var x = 0; x < Width; x++)
                        {
                            for (var l = 0; l < Layers; l++)
                            {
                                bw.Write(_datat2[y, x, l]);
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
                for (var l = 0; l < Layers; l++)
                {
                    var filePath = Path.Combine(storagePath, Name + (Layers > 1 ? "_" + l : "") + ".CSV");
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
                                    dataLineText = dataLineText + _datat1[Height - 1 - y, x, l] + ";";
                                }
                                streamWriter.WriteLine(dataLineText);
                                streamWriter.Flush();
                            }
                            break;
                        case ISOGridType.gridtype2:
                            for (var y = 0; y < Height; y++)
                            {
                                var dataLineText = "";
                                for (var x = 0; x < Width; x++)
                                {
                                    dataLineText = dataLineText + _datat2[Height - 1 - y, x, l] + ";";
                                }
                                streamWriter.WriteLine(dataLineText);
                                streamWriter.Flush();
                            }
                            break;

                    }

                    streamWriter.Close();
                    file.Close();

                }
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
        public void Init(uint width, uint height, byte layers = 1)
        {
            Width = width;
            Height = height;
            Layers = layers;
            switch (Type)
            {
                case ISOGridType.gridtype1:
                    _datat1 = new byte[height, width, layers];
                    break;
                case ISOGridType.gridtype2:
                    _datat2 = new int[height, width, layers];
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
        public int SetValue(uint column, uint row, int value, uint layer = 0)
        {
            if (column < 0 || column >= Width || row < 0 || row >= Height || layer < 0 || layer >= Layers)
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
                    _datat1[row, column, layer] = (byte)value;
                    return 1;
                case ISOGridType.gridtype2:
                    _datat2[row, column, layer] = value;
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
        public int GetValue(uint column, uint row, uint layer)
        {
            if (column < 0 || column >= Width || row < 0 || row >= Height || layer < 0 || layer >= Layers)
            {
                throw new IndexOutOfRangeException();
            }

            switch (Type)
            {
                case ISOGridType.gridtype1:
                    return _datat1[row, column, layer];
                case ISOGridType.gridtype2:
                    return _datat2[row, column, layer];
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}
