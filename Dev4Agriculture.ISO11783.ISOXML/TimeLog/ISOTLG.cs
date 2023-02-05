using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Dev4Agriculture.ISO11783.ISOXML.Exceptions;
using Dev4Agriculture.ISO11783.ISOXML.Messaging;

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


    public partial class ISOTLG
    {

        public string Name;
        public string BinName { get; private set; }
        public string XmlName { get; private set; }
        public string FolderPath;
        public TLGStatus Loaded { get; private set; }
        public TLGDataLogHeader Header { get; private set; }
        public readonly List<TLGDataLogLine> Entries;

        private ISOTLG(string name, string path)
        {
            FolderPath = path;
            Loaded = TLGStatus.INITIAL;
            Name = System.IO.Path.GetFileNameWithoutExtension(name);
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



            if (Utils.AdjustFileNameToIgnoreCasing(FolderPath, BinName, out var binPath))
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



        private bool FindReEntryInBrokenFile(FileStream binaryFile, BinaryReader binaryReader, long filePosition, ushort date)
        {
            filePosition += 6; //We jump over the date Entry of the last line to find the next valid date
            var found = false;
            while (found == false && binaryFile.Position < binaryFile.Length - 2)
            {
                var compareDate = binaryReader.ReadUInt16();
                if (compareDate > date - 1 && compareDate < date + 1)
                {
                    found = true;
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
            long dataLineBeginIndex;
            long lastDataLineBeginIndex = 0;
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
                        var index = binaryFile.Position;
                        if (lastDate == 0)
                        {
                            cause = "No valid StartDate found";
                            quitReading = true;
                        }
                        else if (FindReEntryInBrokenFile(binaryFile, binaryReader, lastDataLineBeginIndex, lastDate) == false)
                        {
                            quitReading = true;
                            cause = "Could not find point for reEntry";
                        }
                        messages.AddError(ResultMessageCode.BINInvalidData,
                            ResultDetail.FromString(Name),
                            ResultDetail.FromNumber(index),
                            ResultDetail.FromNumber(binaryFile.Length),
                            ResultDetail.FromString(cause)
                            );

                        break;
                    case TLGDataLogReadResults.MORE_DATA_THAN_IN_HEADER:
                        index = binaryFile.Position;
                        var solution = "";
                        if (FindReEntryInBrokenFile(binaryFile, binaryReader, lastDataLineBeginIndex, lastDate) == false)
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
                var filePath = storagePath + Name + ".CSV";
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

    }
}
