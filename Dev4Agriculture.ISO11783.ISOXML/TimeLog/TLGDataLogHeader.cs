using System.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using Dev4Agriculture.ISO11783.ISOXML.Messaging;

namespace Dev4Agriculture.ISO11783.ISOXML.TimeLog
{
    public class TLGDataLogHeader
    {
        public List<TLGDataLogDDI> Ddis;
        public List<TLGDataLogPGN> Pgns;
        public TLGGPSOptions GpsOptions;
        public TLGGPSOptions DefaultValueOptions;
        public TLGDataLogLine DefaultValues;

        public byte MaximumNumberOfEntries;

        public TLGDataLogHeader()
        {
            Ddis = new List<TLGDataLogDDI> { };
            Pgns = new List<TLGDataLogPGN> { };
            GpsOptions = new TLGGPSOptions();
            DefaultValueOptions = new TLGGPSOptions();
            DefaultValues = new TLGDataLogLine(byte.MaxValue);//TODO: As we do not know the value yet, we start with the maximum
            MaximumNumberOfEntries = 0;
        }
        internal void ReadElement(XmlNode xmlNode)
        {
            switch (xmlNode.Name)
            {
                case "TIM":

                    break;

                case "PTN":
                    var attribute = xmlNode.Attributes.GetNamedItem("A");
                    if (attribute != null)
                    {
                        if (attribute.Value != "")
                        {
                            GpsOptions.PosNorth = false;
                            DefaultValueOptions.PosNorth = true;
                            DefaultValues.PosNorth = (int)(float.Parse(attribute.Value) * Math.Pow(10, 7));
                        }
                        else
                        {
                            GpsOptions.PosNorth = true;
                        }
                    }

                    attribute = xmlNode.Attributes.GetNamedItem("B");
                    if (attribute != null)
                    {
                        if (attribute.Value != "")
                        {
                            GpsOptions.PosEast = false;
                            DefaultValueOptions.PosEast = true;
                            DefaultValues.PosEast = (int)(float.Parse(attribute.Value) * Math.Pow(10, 7));
                        }
                        else
                        {
                            GpsOptions.PosEast = true;
                        }
                    }

                    attribute = xmlNode.Attributes.GetNamedItem("C");
                    if (attribute != null)
                    {
                        if (attribute.Value != "")
                        {
                            GpsOptions.PosUp = false;
                            DefaultValueOptions.PosUp = true;
                            DefaultValues.PosUp = int.Parse(attribute.Value);
                        }
                        else
                        {
                            GpsOptions.PosUp = true;
                        }
                    }

                    attribute = xmlNode.Attributes.GetNamedItem("D");
                    if (attribute != null)
                    {
                        if (attribute.Value != "")
                        {
                            GpsOptions.PosStatus = false;
                            DefaultValueOptions.PosStatus = true;
                            DefaultValues.PosStatus = byte.Parse(attribute.Value);
                        }
                        else
                        {
                            GpsOptions.PosStatus = true;
                        }
                    }

                    attribute = xmlNode.Attributes.GetNamedItem("E");
                    if (attribute != null)
                    {
                        if (attribute.Value != "")
                        {
                            GpsOptions.Pdop = false;
                            DefaultValueOptions.Pdop = true;
                            DefaultValues.Pdop = ushort.Parse(attribute.Value);
                        }
                        else
                        {
                            GpsOptions.Pdop = true;
                        }
                    }

                    attribute = xmlNode.Attributes.GetNamedItem("F");
                    if (attribute != null)
                    {
                        if (attribute.Value != "")
                        {
                            GpsOptions.Hdop = false;
                            DefaultValueOptions.Hdop = true;
                            DefaultValues.Hdop = ushort.Parse(attribute.Value);
                        }
                        else
                        {
                            GpsOptions.Hdop = true;
                        }
                    }

                    attribute = xmlNode.Attributes.GetNamedItem("G");
                    if (attribute != null)
                    {
                        if (attribute.Value != "")
                        {
                            GpsOptions.NumberOfSatellites = false;
                            DefaultValueOptions.NumberOfSatellites = true;
                            DefaultValues.NumberOfSatellites = byte.Parse(attribute.Value);
                        }
                        else
                        {
                            GpsOptions.NumberOfSatellites = true;
                        }
                    }

                    attribute = xmlNode.Attributes.GetNamedItem("H");
                    if (attribute != null)
                    {
                        if (attribute.Value != "")
                        {
                            GpsOptions.GpsUTCTime = false;
                            DefaultValueOptions.GpsUTCTime = true;
                            DefaultValues.GpsUTCTime = uint.Parse(attribute.Value);
                        }
                        else
                        {
                            GpsOptions.GpsUTCTime = true;
                        }
                    }


                    attribute = xmlNode.Attributes.GetNamedItem("I");
                    if (attribute != null)
                    {
                        if (attribute.Value != "")
                        {
                            GpsOptions.GpsUTCDate = false;
                            DefaultValueOptions.GpsUTCDate = true;
                            DefaultValues.GpsUTCDate = ushort.Parse(attribute.Value);
                        }
                        else
                        {
                            GpsOptions.GpsUTCDate = true;
                        }
                    }

                    break;
                case "DLV":
                    ushort ddi = 0;
                    attribute = xmlNode.Attributes.GetNamedItem("A");
                    if (attribute != null && attribute.Value != "")
                    {
                        ddi = ushort.Parse(attribute.Value, System.Globalization.NumberStyles.HexNumber);
                    }
                    else
                    {
                        break;
                    }

                    if (ddi != (ushort)DDILIST.DDI_PGN)
                    {
                        var tLGDataDDIEntry = new TLGDataLogDDI
                        {
                            Ddi = ddi
                        };

                        attribute = xmlNode.Attributes.GetNamedItem("B");
                        if (attribute != null && attribute.Value != "")
                        {
                            tLGDataDDIEntry.DefaultValue = int.Parse(attribute.Value);
                            tLGDataDDIEntry.HasDefaultValue = true;

                        }
                        else
                        {
                            tLGDataDDIEntry.HasDefaultValue = false;
                        }


                        attribute = xmlNode.Attributes.GetNamedItem("C");
                        if (attribute != null && attribute.Value != "")
                        {
                            tLGDataDDIEntry.DeviceElement = short.Parse(attribute.Value.Substring(3));
                        }
                        else
                        {
                            //TODO: Add an exception here
                            break;
                        }
                        tLGDataDDIEntry.Index = MaximumNumberOfEntries;
                        MaximumNumberOfEntries++;
                        Ddis.Add(tLGDataDDIEntry);
                    }
                    else
                    {
                        var tlgDataLogPGN = new TLGDataLogPGN();

                        attribute = xmlNode.Attributes.GetNamedItem("B");
                        if (attribute != null && attribute.Value != "")
                        {
                            tlgDataLogPGN.DefaultValue = int.Parse(attribute.Value);
                            tlgDataLogPGN.HasDefaultValue = true;

                        }
                        else
                        {
                            tlgDataLogPGN.HasDefaultValue = false;
                        }

                        attribute = xmlNode.Attributes.GetNamedItem("D");
                        if (attribute != null && attribute.Value != "")
                        {
                            tlgDataLogPGN.DataLogPGN = uint.Parse(attribute.Value);
                        }
                        else
                        {
                            break;
                        }

                        attribute = xmlNode.Attributes.GetNamedItem("E");
                        if (attribute != null && attribute.Value != "")
                        {
                            tlgDataLogPGN.StartBit = byte.Parse(attribute.Value);
                        }
                        else
                        {
                            break;
                        }

                        attribute = xmlNode.Attributes.GetNamedItem("F");
                        if (attribute != null && attribute.Value != "")
                        {
                            tlgDataLogPGN.StopBit = byte.Parse(attribute.Value);
                        }
                        else
                        {
                            break;
                        }

                        tlgDataLogPGN.Index = MaximumNumberOfEntries;
                        MaximumNumberOfEntries++;
                        Pgns.Add(tlgDataLogPGN);

                    }

                    break;
            }

            if (xmlNode.ChildNodes.Count > 0)
            {
                foreach (XmlNode child in xmlNode.ChildNodes)
                {
                    ReadElement(child);
                }
            }
        }

        public string ToStringWithDDIsOnly()
        {
            var text = "Date;Time;";
            if (GpsOptions.PosNorth)
            {
                text += "Latitude;";
            }
            if (GpsOptions.PosEast)
            {
                text += "Longitude;";
            }

            if (GpsOptions.PosUp)
            {
                text += "Up;";
            }


            if (GpsOptions.PosStatus)
            {
                text += "Status;";
            }


            if (GpsOptions.Pdop)
            {
                text += "PDOP;";
            }

            if (GpsOptions.Hdop)
            {
                text += "HDOP;";
            }

            if (GpsOptions.NumberOfSatellites)
            {
                text += "NumberOfSatellites;";
            }


            if (GpsOptions.GpsUTCTime)
            {
                text += "UTCTime;";
            }

            if (GpsOptions.GpsUTCDate)
            {
                text += "UTCDate;";
            }


            foreach (var ddi in Ddis)
            {
                text += "'" + ddi.Ddi + "/" + ddi.DeviceElement + ";";
            }
            return text;
        }

        internal static ResultWithMessages<TLGDataLogHeader> Load(string path, string name)
        {
            var tlgHeader = new TLGDataLogHeader();
            var filePath = path + name;
            if (File.Exists(filePath))
            {
                var xmlDocument = new XmlDocument();
                try
                {
                    xmlDocument.Load(filePath);
                    tlgHeader.ReadElement(xmlDocument.DocumentElement);
                    return new ResultWithMessages<TLGDataLogHeader>(tlgHeader);
                }
                catch ( Exception e)
                {
                    var result = new ResultWithMessages<TLGDataLogHeader>(null);
                    result.Messages.Add(new ResultMessage(ResultMessageType.Error, "Could not read Header " + name + ", Exception: " + e.ToString()));
                    return new ResultWithMessages<TLGDataLogHeader>(null);
                }
            }
            return new ResultWithMessages<TLGDataLogHeader>(tlgHeader);
        }
    }
}
