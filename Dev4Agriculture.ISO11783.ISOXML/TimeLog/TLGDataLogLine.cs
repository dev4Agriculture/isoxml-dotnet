using System;
using System.IO;
using Dev4Agriculture.ISO11783.ISOXML.Utils;

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

    public struct TLGDataLogEntry
    {
        public bool IsSet;
        public int Value;
    }


    public partial class TLGDataLogLine
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

        public DateTime DateTime
        {
            get => DateUtilities.GetDateTimeFromTimeLogInfos(Date, Time);
            set
            {
                Date = DateUtilities.GetDaysSince1980(value);
                Time = DateUtilities.GetMilliSecondsInDay(value);
            }
        }

        public double Latitude
        {
            get => PosNorth / ISOTLG.TLG_GPS_FACTOR;
            set => PosNorth = (int)(value * ISOTLG.TLG_GPS_FACTOR);
        }

        public double Longitude
        {
            get => PosEast / ISOTLG.TLG_GPS_FACTOR;
            set => PosEast = (int)(value * ISOTLG.TLG_GPS_FACTOR);
        }



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

            if (header.GpsOptions.NumberOfSatellites)
            {
                if (file.Position + 1 >= file.Length)
                {
                    return TLGDataLogReadResults.FILE_END;
                }
                NumberOfSatellites = binaryReader.ReadByte();
            }
            else if (header.DefaultValueOptions.NumberOfSatellites)
            {
                NumberOfSatellites = header.DefaultValues.NumberOfSatellites;
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

            if (header.GpsOptions.NumberOfSatellites)
            {
                binaryWriter.Write(NumberOfSatellites);
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


        public bool Has(uint index)
        {
            return index < ArraySize && Entries[index].IsSet;
        }


        public int Get(uint index)
        {
            return Entries[index].Value;
        }

        public bool TryGetValue(uint index, out int value)
        {
            if (index >= 0 && index < ArraySize && Entries[index].IsSet)
            {
                value = Entries[index].Value;
                return true;
            }
            value = 0;
            return false;
        }

    }
}
