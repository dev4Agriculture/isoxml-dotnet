using System;
using System.Collections.Generic;
using System.IO;
using Dev4Agriculture.ISO11783.ISOXML.Messaging;

namespace Dev4Agriculture.ISO11783.ISOXML.TimeLog
{
    public enum DDILIST : ushort
    {
        DDI_PGN = 57342
    }

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


    public class ISOTLG
    {
        public const double GPS_FACTOR = 10000000.0;

        public string Name;
        public string BinName;
        public string XmlName;
        public string Path;
        public TLGStatus Loaded;
        public TLGDataLogHeader Header { get; private set; }
        private readonly List<TLGDataLogLine> _entries;

        private ISOTLG(string name, string path)
        {
            Path = path;
            Loaded = TLGStatus.INITIAL;
            Name = System.IO.Path.GetFileNameWithoutExtension(name);
            BinName = Name + ".BIN";
            XmlName = Name + ".XML";
            Header = new TLGDataLogHeader();
            _entries = new List<TLGDataLogLine>();
        }

        internal string GetPath()
        {
            if (Path.EndsWith("\\") == false)
            {
                Path += "\\";
            }

            return Path;
        }

        public bool IsComplete()
        {
            if (File.Exists(GetPath() + BinName) == false ||
                File.Exists(GetPath() + XmlName) == false)
            {
                return false;
            }
            else
            {
                return true;
            }
        }



        private List<ResultMessage> LoadData()
        {
            var headerResult = TLGDataLogHeader.Load(GetPath(), XmlName);
            if (headerResult.Result == null)
            {
                this.Loaded = TLGStatus.ERROR;

                return headerResult.Messages;
            }
            Header = headerResult.Result;
            var messages = headerResult.Messages;



            var filePath = GetPath() + BinName;
            if (File.Exists(filePath))
            {
                var binaryFile = File.Open(filePath, FileMode.Open);
                messages.AddRange( ReadBinaryData(binaryFile, Header));
                binaryFile.Close();
                Loaded = TLGStatus.LOADED;
            }
            else
            {
                messages.Add(new ResultMessage(ResultMessageType.Error, "Missing TimeLog Binary File " + BinName));
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

        internal string GetKMLLineString()
        {
            var content = "";

            foreach (var entry in _entries)
            {
                /*if( ( (GPSQuality)entry.posStatus != GPSQuality.ERROR) &&
                    ( (GPSQuality)entry.posStatus != GPSQuality.UNKNOWN)
                    )
                {*/
                content += "\n" +
                    (entry.PosEast / GPS_FACTOR).ToString().Replace(",", ".") + "," +
                    (entry.PosNorth / GPS_FACTOR).ToString().Replace(",", ".") + "," +
                    entry.PosUp.ToString();
                //}
            }


            return content;
        }
        internal void WriteBinaryData(FileStream binaryFile, TLGDataLogHeader header)
        {
            var binaryWriter = new BinaryWriter(binaryFile);
            foreach (var line in _entries)
            {
                line.WriteLine(header, binaryWriter);
            }
            binaryFile.Close();
        }


        internal List<ResultMessage> ReadBinaryData(FileStream binaryFile, TLGDataLogHeader header)
        {
            var messages = new List<ResultMessage>();
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
                        _entries.Add(tlgDataLogLine);
                        lastDataLineBeginIndex = dataLineBeginIndex;
                        lastDate = tlgDataLogLine.Date;
                        break;
                    case TLGDataLogReadResults.FILE_END_OK:
                        _entries.Add(tlgDataLogLine);
                        quitReading = true;
                        break;
                    case TLGDataLogReadResults.INVALID_DATA:
                        messages.Add(new ResultMessage(ResultMessageType.Error,
                                    "We found invalid Data. File: " + Name + " Position: " + binaryFile.Position + " of " + binaryFile.Length
                                    ));
                        if (lastDate == 0)
                        {
                            Console.WriteLine("ERROR: No valid StartDate found");
                            quitReading = true;
                        }
                        else if (FindReEntryInBrokenFile(binaryFile, binaryReader, lastDataLineBeginIndex, lastDate) == false)
                        {
                            quitReading = true;
                            Console.WriteLine("Could not find point for reEntry");
                        }
                        break;
                    case TLGDataLogReadResults.MORE_DATA_THAN_IN_HEADER:
                        messages.Add(
                            new ResultMessage(
                                ResultMessageType.Error,
                                "There were more binary data linked in the binary file than exist in the header")
                            );
                        if (FindReEntryInBrokenFile(binaryFile, binaryReader, lastDataLineBeginIndex, lastDate) == false)
                        {
                            quitReading = true;
                            Console.WriteLine("Could not find point for reEntry");
                        }
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
            try
            {
                var filePath = storagePath + Name + ".BIN";
                var file = File.Create(filePath);
                WriteBinaryData(file, Header);
            }
            catch (Exception e)
            {
                //TODO Add Error or throw Exception
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
                foreach (var entry in _entries)
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
