using System;
using System.IO;

namespace Dev4Agriculture.ISO11783.ISOXML.TimeLog
{

    public enum TLGDataLogReadResults
    {
        OK,
        FILE_END_OK,
        INVALID_DATA,
        MORE_DATA_THAN_IN_HEADER,
        FILE_END
    }

    public class TLGDataLogEntry
    {
        public bool IsSet;
        public int Value;
    }

    public class TLGDataLogLine
    {
        public uint Time;
        public ushort Date;
        public int PosNorth;
        public int PosEast;
        public int PosUp;
        public byte PosStatus;
        public ushort Pdop;
        public ushort Hdop;
        public byte NumberOfSatellites;
        public uint GpsUTCTime;
        public ushort GpsUTCDate;

        public byte NumberOfEntries;
        public byte ArraySize;
        public TLGDataLogEntry[] Entries;

        public TLGDataLogLine(byte arraySize)
        {
            ArraySize = arraySize;
            Entries = new TLGDataLogEntry[arraySize];
            for (byte index = 0; index < arraySize; index++)
            {
                Entries[index] = new TLGDataLogEntry();
            }
        }


        public TLGDataLogReadResults ReadLine(TLGDataLogHeader header, BinaryReader binaryReader, FileStream file, ushort lastDate)
        {
            if (file.Position + 6 >= file.Length)
            {
                return TLGDataLogReadResults.FILE_END;
            }
            Time = binaryReader.ReadUInt32();
            Date = binaryReader.ReadUInt16();
            if (lastDate != 0 && (Date < lastDate - 1 || Date > lastDate + 1))
            {
                return TLGDataLogReadResults.INVALID_DATA;
            }

            if (header.GpsOptions.PosNorth)
            {
                if (file.Position + 4 >= file.Length)
                {
                    return TLGDataLogReadResults.FILE_END;
                }
                PosNorth = binaryReader.ReadInt32();
            }
            else if (header.DefaultValueOptions.PosNorth)
            {
                PosNorth = header.DefaultValues.PosNorth;
            }

            if (header.GpsOptions.PosEast)
            {
                if (file.Position + 4 >= file.Length)
                {
                    return TLGDataLogReadResults.FILE_END;
                }
                PosEast = binaryReader.ReadInt32();
            }
            else if (header.DefaultValueOptions.PosEast)
            {
                PosEast = header.DefaultValues.PosEast;
            }

            if (header.GpsOptions.PosUp)
            {
                if (file.Position + 4 >= file.Length)
                {
                    return TLGDataLogReadResults.FILE_END;
                }
                PosUp = binaryReader.ReadInt32();
            }
            else if (header.DefaultValueOptions.PosUp)
            {
                PosUp = header.DefaultValues.PosUp;
            }

            if (header.GpsOptions.PosStatus)
            {
                if (file.Position + 1 >= file.Length)
                {
                    return TLGDataLogReadResults.FILE_END;
                }
                PosStatus = binaryReader.ReadByte();
            }
            else if (header.DefaultValueOptions.PosStatus)
            {
                PosStatus = header.DefaultValues.PosStatus;
            }



            if (header.GpsOptions.Pdop)
            {
                if (file.Position + 2 >= file.Length)
                {
                    return TLGDataLogReadResults.FILE_END;
                }
                Pdop = binaryReader.ReadUInt16();
            }
            else if (header.DefaultValueOptions.Pdop)
            {
                Pdop = header.DefaultValues.Pdop;
            }


            if (header.GpsOptions.Hdop)
            {
                if (file.Position + 2 >= file.Length)
                {
                    return TLGDataLogReadResults.FILE_END;
                }
                Hdop = binaryReader.ReadUInt16();
            }
            else if (header.DefaultValueOptions.Hdop)
            {
                Hdop = header.DefaultValues.Hdop;
            }

            if (header.GpsOptions.GpsUTCTime)
            {
                if (file.Position + 4 >= file.Length)
                {
                    return TLGDataLogReadResults.FILE_END;
                }
                GpsUTCTime = binaryReader.ReadUInt32();
            }
            else if (header.DefaultValueOptions.GpsUTCTime)
            {
                GpsUTCTime = header.DefaultValues.GpsUTCTime;
            }


            if (header.GpsOptions.GpsUTCDate)
            {
                if (file.Position + 2 >= file.Length)
                {
                    return TLGDataLogReadResults.FILE_END;
                }
                GpsUTCDate = binaryReader.ReadUInt16();
            }
            else if (header.DefaultValueOptions.GpsUTCDate)
            {
                GpsUTCDate = header.DefaultValues.GpsUTCDate;
            }

            NumberOfEntries = binaryReader.ReadByte();
            for (var index = 0; index < NumberOfEntries; index++)
            {
                if (file.Position + 5 > file.Length)
                {
                    return TLGDataLogReadResults.FILE_END;
                }
                var dataLogIndex = binaryReader.ReadByte();
                if (dataLogIndex >= ArraySize)
                {
                    return TLGDataLogReadResults.MORE_DATA_THAN_IN_HEADER;
                }
                Entries[dataLogIndex].IsSet = true;
                Entries[dataLogIndex].Value = binaryReader.ReadInt32();
            }


            if (file.Position == file.Length)
            {
                return TLGDataLogReadResults.FILE_END_OK;
            }


            return TLGDataLogReadResults.OK;

        }

        internal void WriteLine(TLGDataLogHeader header, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Time);
            binaryWriter.Write(Date);
            if (header.GpsOptions.PosNorth)
            {
                binaryWriter.Write(PosNorth);
            }

            if (header.GpsOptions.PosEast)
            {
                binaryWriter.Write(PosEast);
            }

            if (header.GpsOptions.PosUp)
            {
                binaryWriter.Write((uint)PosUp);
            }

            if (header.GpsOptions.PosStatus)
            {
                binaryWriter.Write(PosStatus);
            }


            if (header.GpsOptions.Pdop)
            {
                binaryWriter.Write(Pdop);
            }


            if (header.GpsOptions.Hdop)
            {
                binaryWriter.Write(Hdop);
            }

            if (header.GpsOptions.GpsUTCTime)
            {
                binaryWriter.Write(GpsUTCTime);
            }


            if (header.GpsOptions.GpsUTCDate)
            {
                binaryWriter.Write(GpsUTCDate);
            }

            binaryWriter.Write(NumberOfEntries);
            for (byte index = 0; index < Entries.Length; index++)
            {
                if (Entries[index].IsSet)
                {
                    binaryWriter.Write(index);
                    binaryWriter.Write(Entries[index].Value);
                }
            }

        }

        public string ToStringWithDDIsOnly(TLGDataLogHeader header)
        {
            var text = "" +
                DateUtilities.GetDateFromDaysSince1980(Date) + ";" +
                DateUtilities.GetTimeFromMilliSeconds(Time) + ";";
            if (header.GpsOptions.PosNorth)
            {
                text += PosNorth * Math.Pow(10, -7) + ";";
            }
            if (header.GpsOptions.PosEast)
            {
                text += PosEast * Math.Pow(10, -7) + ";";
            }

            if (header.GpsOptions.PosUp)
            {
                text += PosUp + ";";
            }


            if (header.GpsOptions.PosStatus)
            {
                text += PosStatus;
            }


            if (header.GpsOptions.Pdop)
            {
                text += Pdop;
            }

            if (header.GpsOptions.Hdop)
            {
                text += Hdop;
            }

            if (header.GpsOptions.NumberOfSatellites)
            {
                text += NumberOfSatellites;
            }


            if (header.GpsOptions.GpsUTCTime)
            {
                text += GpsUTCTime;
            }

            if (header.GpsOptions.GpsUTCDate)
            {
                text += GpsUTCDate;
            }


            foreach (var ddi in header.Ddis)
            {
                text += ";";
                if (Entries[ddi.Index].IsSet)
                {
                    text += Entries[ddi.Index].Value;
                }
            }
            return text;
        }
    }
}
