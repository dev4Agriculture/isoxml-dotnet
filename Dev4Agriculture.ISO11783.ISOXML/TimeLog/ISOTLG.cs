using System;
using System.Collections.Generic;
using System.IO;
using Dev4Agriculture.ISO11783.ISOXML.Exceptions;
using Dev4Agriculture.ISO11783.ISOXML.Messaging;
using Dev4Agriculture.ISO11783.ISOXML.Utils;

namespace Dev4Agriculture.ISO11783.ISOXML.TimeLog
{


    public enum GPSQuality : byte
    {
        NO_GPS,            // 0 = no GPS fix
        GNSS_FIX,           //1 = GNSS fix
        DGNSS_FIX,          //2 = DGNSS fix
        PRECISE_GNSS,       //3 = Precise GNSS, no deliberate degradation
        RTK_FIX_INTEGER,    //4 = RTK Fixed Integer
        RTK_FLOAT,          //5 = RTK Float
        EST_DR_MODE,        //6 = Est(DR)mode
        MANUAL_INPUT,       //7 = Manual input
        SIMULATION_MODE,    //8 = Simulate mode
        RESERVED_9,
        RESERVED_10,
        RESERVED_11,
        RESERVED_12,
        RESERVED_13,        //9–13 = Reserved
        ERROR,              //14 = Error
        UNKNOWN             //15 = PositionStatus unknown
    }

    public enum TLGStatus
    {
        INITIAL,
        LOADED,
        ERROR
    }

    public enum TriggerMethods
    {
        OnTime = 1,
        OnDistance = 2,
        ThresholdLimits = 4,
        OnChange = 8,
        Total = 16

    }

    public class ISOTLGReadConfiguration
    {
        public int DaysMinForValidDateRange;
        public int DaysMaxForValidDateRange;
    }


    public partial class ISOTLG
    {
        private static ISOTLGReadConfiguration s_readConfiguration = new ISOTLGReadConfiguration()
        {
            DaysMinForValidDateRange = DateUtilities.DAYS_MIN_FOR_VALID_DATE_RANGE,
            DaysMaxForValidDateRange = DateUtilities.DAYS_MAX_FOR_VALID_DATE_RANGE
        };

        public static void SetReaderConfiguration(ISOTLGReadConfiguration readConfiguration)
        {
            s_readConfiguration = readConfiguration;
        }

        public string Name;
        public string BinName { get; private set; }
        public string XmlName { get; private set; }
        public string FolderPath;
        public TLGStatus Loaded { get; private set; }
        public TLGDataLogHeader Header { get; private set; }
        public readonly List<TLGDataLogLine> Entries;

        private ISOTLG(int index, string path)
        {
            Name = path;
            Loaded = TLGStatus.INITIAL;
            Entries = new List<TLGDataLogLine>();
            Name = "TLG" + index.ToString().PadLeft(5, '0');
            BinName = Name + ".bin";
            XmlName = Name + ".xml";
            Header = new TLGDataLogHeader();
        }
        private ISOTLG(string name, string path)
        {
            FolderPath = path;
            Loaded = TLGStatus.INITIAL;
            Name = Path.GetFileNameWithoutExtension(name);
            BinName = Name + ".bin";
            XmlName = Name + ".xml";
            Header = new TLGDataLogHeader();
            Entries = new List<TLGDataLogLine>();
        }



        private ResultMessageList LoadData()
        {
            var headerResult = TLGDataLogHeader.Load(FolderPath, XmlName);
            if (headerResult.Result == null)
            {
                Loaded = TLGStatus.ERROR;

                return headerResult.Messages;
            }
            Header = headerResult.Result;
            var messages = headerResult.Messages;


            if (FileUtils.HasMultipleFilesEndingWithThatName(FolderPath, BinName))
            {
                messages.AddWarning(ResultMessageCode.FileNameEndingMultipleTimes, ResultDetail.FromString(BinName));
            }

            if (FileUtils.AdjustFileNameToIgnoreCasing(FolderPath, BinName, out var binPath))
            {
                var binaryFile = File.Open(binPath, FileMode.Open);
                messages.AddRange(ReadBinaryData(binaryFile, Header));
                binaryFile.Close();
                Loaded = TLGStatus.LOADED;
            }
            else
            {
                messages.AddError(ResultMessageCode.FileNotFound,
                    ResultDetail.FromString(BinName)
                    );
                Loaded = TLGStatus.ERROR;

            }


            return messages;
        }



        private bool FindReEntryInBrokenFile(FileStream binaryFile, BinaryReader binaryReader, ushort date)
        {
            //Read one Byte to go forward before searching a valid date
            var justToRead = binaryReader.ReadByte();
            var found = false;
            while (found == false && binaryFile.Position < binaryFile.Length - 6)
            {
                var compareTime = binaryReader.ReadUInt32();
                var compareDate = binaryReader.ReadUInt16();
                if ((compareDate >= date - 1) && (compareDate <= date + 1) && compareTime >= 0 && compareTime <= DateUtilities.MILLISECONDS_IN_DAY)
                {
                    found = true;
                }
                else
                {
                    binaryFile.Seek(-5, SeekOrigin.Current);
                }
            }

            if (found)
            {

                binaryFile.Seek(-6, SeekOrigin.Current);
            }

            return found;

        }


        internal void WriteBinaryData(FileStream binaryFile, TLGDataLogHeader header)
        {
            var binaryWriter = new BinaryWriter(binaryFile);
            foreach (var line in Entries)
            {
                line.WriteLine(header, binaryWriter);
            }
            binaryFile.Close();
        }

        private ResultMessageList ReadBinaryData(FileStream binaryFile, TLGDataLogHeader header)
        {
            var messages = new ResultMessageList();
            var binaryReader = new BinaryReader(binaryFile);
            ushort lastDate = 0;
            long dataLineBeginIndex = 0;
            long lastDataLineBeginIndex = 0;

            if (binaryFile.Length == 0)
            {
                messages.AddWarning(ResultMessageCode.BINEmptyFile,
                                            ResultDetail.FromString(Name),
                                            ResultDetail.FromNumber(0),
                                            ResultDetail.FromNumber(binaryFile.Length),
                                            ResultDetail.FromString("An empty BIN-File might cause confusion")
                                            );
                return messages;
            }

            if (binaryFile.Length < 6)
            {
                messages.AddError(ResultMessageCode.BINInvalidData,
                                            ResultDetail.FromString(Name),
                                            ResultDetail.FromNumber(0),
                                            ResultDetail.FromNumber(binaryFile.Length),
                                            ResultDetail.FromString("The file is smaller than 6 bytes")
                                            );
                return messages;
            }

            //Check the first few bytes to be correct and within a logical range. For some rare broken files, this is a good solution to restore broken data
            bool valid = false;
            do
            {
                var timeStamp = binaryReader.ReadInt32();
                var date = binaryReader.ReadInt16();

                if (timeStamp < 0 || timeStamp > DateUtilities.MILLISECONDS_IN_DAY || date < s_readConfiguration.DaysMinForValidDateRange || date > s_readConfiguration.DaysMaxForValidDateRange)
                {
                    dataLineBeginIndex += 1;
                    binaryFile.Seek(-5, SeekOrigin.Current);//In total we move 1 byte less backwards than we moved forward before

                }
                else
                {
                    binaryFile.Seek(-6, SeekOrigin.Current);
                    valid = true;
                }
                if ((binaryFile.Length - binaryFile.Position) < 6)
                {
                    messages.AddError(ResultMessageCode.BINInvalidData,
                                                ResultDetail.FromString(Name),
                                                ResultDetail.FromNumber(0),
                                                ResultDetail.FromNumber(binaryFile.Length),
                                                ResultDetail.FromString("Could not find point for reEntry")
                                                );
                    return messages;
                }
            } while (valid == false);
            if (binaryFile.Position > 0)
            {
                messages.AddError(ResultMessageCode.BINInvalidData,
                                            ResultDetail.FromString(Name),
                                            ResultDetail.FromNumber(0),
                                            ResultDetail.FromNumber(binaryFile.Length),
                                            ResultDetail.FromString("Could find first valid date at " + binaryFile.Position)
                                            );
            }


            while (binaryFile.Position < binaryFile.Length)
            {
                var quitReading = false;
                dataLineBeginIndex = binaryFile.Position;
                var tlgDataLogLine = new TLGDataLogLine(Header.MaximumNumberOfEntries);
                switch (tlgDataLogLine.ReadLine(header, binaryReader, binaryFile, lastDate))
                {
                    case TLGDataLogReadResults.OK:
                        Entries.Add(tlgDataLogLine);
                        lastDataLineBeginIndex = dataLineBeginIndex;
                        lastDate = tlgDataLogLine.Date;
                        break;
                    case TLGDataLogReadResults.FILE_END_OK:
                        Entries.Add(tlgDataLogLine);
                        quitReading = true;
                        break;
                    case TLGDataLogReadResults.INVALID_DATA:
                        var cause = "";
                        binaryFile.Seek(dataLineBeginIndex, SeekOrigin.Begin);
                        var index = binaryFile.Position;
                        if (lastDate == 0)
                        {
                            cause = "No valid StartDate found";
                            quitReading = true;
                        }
                        else if (FindReEntryInBrokenFile(binaryFile, binaryReader, lastDate) == false)
                        {
                            quitReading = true;
                            cause = "Could not find point for reEntry";
                        }
                        else
                        {
                            cause = "Found reEntry at position " + binaryFile.Position;
                        }
                        messages.AddError(ResultMessageCode.BINInvalidData,
                            ResultDetail.FromString(Name),
                            ResultDetail.FromNumber(index),
                            ResultDetail.FromNumber(binaryFile.Length),
                            ResultDetail.FromString(cause)
                            );

                        break;
                    case TLGDataLogReadResults.MORE_DATA_THAN_IN_HEADER:
                        var solution = "";
                        binaryFile.Seek(dataLineBeginIndex, SeekOrigin.Begin);
                        index = binaryFile.Position;
                        if (FindReEntryInBrokenFile(binaryFile, binaryReader, lastDate) == false)
                        {
                            quitReading = true;
                            solution = "DataLoss, could not find point for reEntry";
                        }
                        else
                        {
                            solution = $"Reentry at  {binaryFile.Position}";
                        }
                        messages.AddError(ResultMessageCode.BINInvalidNumberOfDataInRow,
                                ResultDetail.FromString(Name),
                                ResultDetail.FromNumber(binaryFile.Position),
                                ResultDetail.FromString(solution)
                            );
                        break;
                    case TLGDataLogReadResults.FILE_END:
                        quitReading = true;
                        break;
                }
                if (quitReading)
                {
                    break;
                }
            }
            binaryFile.Close();

            return messages;
        }

        internal void SaveTLG(string storagePath)
        {
            var filePath = Path.Combine(storagePath, Name);
            try
            {
                var file = File.Create(filePath + ".bin");
                WriteBinaryData(file, Header);
                Header.Save(filePath + ".xml");
            }
            catch (Exception e)
            {
                throw new CouldNotStoreTLGException("Could not store " + filePath, e);
            }
        }
        public void SaveCSV(string storagePath)
        {
            try
            {
                var filePath = Path.Combine(storagePath, Name + ".CSV");
                var file = File.Create(filePath);
                var streamWriter = new StreamWriter(file);
                streamWriter.WriteLine(Header.ToStringWithDDIsOnly());
                foreach (var entry in Entries)
                {
                    streamWriter.WriteLine(entry.ToStringWithDDIsOnly(Header));
                }

                streamWriter.Flush();
                streamWriter.Close();
                file.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not open file " + storagePath + "; canceling to write: " + e.ToString());
                throw e;
            }
        }


        public static ResultWithMessages<ISOTLG> LoadTLG(string name, string rootFolder)
        {
            var tlg = new ISOTLG(name, rootFolder);
            var result = new ResultWithMessages<ISOTLG>(tlg);
            result.Messages.AddRange(tlg.LoadData());
            return result;
        }

        public static ISOTLG Generate(int index, string path, TLGGPSOptions tlgGPSOptions = null)
        {
            if (tlgGPSOptions == null)
            {
                tlgGPSOptions = new TLGGPSOptions
                {
                    PosEast = true,
                    PosNorth = true,
                    PosStatus = true
                };
            }

            var tlg = new ISOTLG(index, path);
            tlg.Header.GpsOptions = tlgGPSOptions;

            return tlg;
        }

        public static int ConvertGPS(decimal pos) => Convert.ToInt32(pos * (int)TLG_GPS_FACTOR);

    }
}
