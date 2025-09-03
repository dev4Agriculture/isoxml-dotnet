using System;
using System.IO;
using System.Runtime.ConstrainedExecution;
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

                // Stateless accessor class for zero-allocation API compatibility
    public class TLGDataLogEntryAccessor
    {
        private readonly TLGDataLogLine _parent;
        private readonly int _index;

        public TLGDataLogEntryAccessor(TLGDataLogLine parent, int index)
        {
            _parent = parent;
            _index = index;
        }

        public bool IsSet
        {
            get => _parent.EntrySet[_index];
            set => _parent.EntrySet[_index] = value;
        }

        public int Value
        {
            get => _parent.EntryValues[_index];
            set
            {
                _parent.EntryValues[_index] = value;
                _parent.EntrySet[_index] = true;
            }
        }

        /// <summary>
        /// Unsets this entry (marks as not set). This method provides zero-allocation access.
        /// </summary>
        public void UnSet() => _parent.EntrySet[_index] = false;
    }

    // Zero-allocation collection wrapper
    public class TLGDataLogEntriesCollection
    {
        private readonly TLGDataLogLine _parent;

        public TLGDataLogEntriesCollection(TLGDataLogLine parent)
        {
            _parent = parent;
        }

        public TLGDataLogEntryAccessor this[int index] => new TLGDataLogEntryAccessor(_parent, index);
        public int Length => _parent.ArraySize;
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

        // Parallel arrays (Approach 2) - internal to allow accessor access
        internal int[] EntryValues;
        internal bool[] EntrySet;

        // Zero-allocation collection wrapper
        private TLGDataLogEntriesCollection _entriesCollection;


        /// <summary>
        /// Provides array-like access to TLG data log entries.
        /// WARNING: This property creates temporary accessor objects on each access, which can impact performance.
        /// For better performance, use the direct access methods: IsEntrySet(), GetEntryValue(), SetEntryValue(), UnSetValue()
        /// </summary>
        [Obsolete("This property creates temporary accessor objects on each access, which can impact performance. Use direct access methods (IsEntrySet, GetEntryValue, SetEntryValue, UnSetValue) for better performance.", false)]
        public TLGDataLogEntriesCollection Entries
        {
            get
            {
                if (_entriesCollection == null)
                {
                    _entriesCollection = new TLGDataLogEntriesCollection(this);
                }
                return _entriesCollection;
            }
        }

        // Direct access methods for zero-allocation access (recommended for performance)
        /// <summary>
        /// Gets whether the entry at the specified index is set. This method provides zero-allocation access.
        /// </summary>
        public bool IsEntrySet(int index) => EntrySet[index];

        /// <summary>
        /// Gets the value of the entry at the specified index. This method provides zero-allocation access.
        /// </summary>
        public int GetEntryValue(int index) => EntryValues[index];

        /// <summary>
        /// Sets the value of the entry at the specified index and automatically marks it as set. This method provides zero-allocation access.
        /// </summary>
        public void SetEntryValue(int index, int value)
        {
            EntryValues[index] = value;
            EntrySet[index] = true;
        }

        /// <summary>
        /// Unsets the entry at the specified index (marks as not set). This method provides zero-allocation access.
        /// </summary>
        public void UnSetValue(int index) => EntrySet[index] = false;


        public int CountEntries() => EntryValues.Length;

        public TLGDataLogLine(byte arraySize)
        {
            ArraySize = arraySize;
            EntryValues = new int[arraySize];
            EntrySet = new bool[arraySize];
        }

        public TLGDataLogLine(TLGDataLogLine input)
        {
            Date = input.Date;
            GpsUTCDate = input.GpsUTCDate;
            GpsUTCTime = input.GpsUTCTime;
            NumberOfEntries = input.NumberOfEntries;
            NumberOfSatellites = input.NumberOfSatellites;
            Hdop = input.Hdop;
            Pdop = input.Pdop;
            Time = input.Time;
            PosStatus = input.PosStatus;
            PosUp = input.PosUp;
            PosEast = input.PosEast;
            PosNorth = input.PosNorth;
            ArraySize = input.ArraySize;
            EntryValues = new int[ArraySize];
            EntrySet = new bool[ArraySize];
            for (byte index = 0; index < ArraySize; index++)
            {
                EntrySet[index] = input.EntrySet[index];
                EntryValues[index] = input.EntryValues[index];
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
                EntrySet[dataLogIndex] = true;
                EntryValues[dataLogIndex] = binaryReader.ReadInt32();
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
            for (byte index = 0; index < ArraySize; index++)
            {
                if (EntrySet[index])
                {
                    binaryWriter.Write(index);
                    binaryWriter.Write(EntryValues[index]);
                }
            }

        }


        public bool Has(uint index)
        {
            return index < ArraySize && EntrySet[index];
        }


        public int Get(uint index)
        {
            return EntryValues[index];
        }

        public bool TryGetValue(uint index, out int value)
        {
            if (index >= 0 && index < ArraySize && EntrySet[index])
            {
                value = EntryValues[index];
                return true;
            }
            value = 0;
            return false;
        }

    }
}
