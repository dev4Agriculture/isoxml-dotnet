using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Dev4Agriculture.ISO11783.ISOXML.IdHandling;
using Dev4Agriculture.ISO11783.ISOXML.LinkListFile;
using Dev4Agriculture.ISO11783.ISOXML.Messaging;
using Dev4Agriculture.ISO11783.ISOXML.Serializer;

namespace Dev4Agriculture.ISO11783.ISOXML
{


    public class IsoLinkList
    {
        private static readonly LinkListSerializer LinkListSerializer = new LinkListSerializer();

        private readonly ISO11783LinkListFile _linkListContent;
        private readonly IdList _groupIds;


        public ISO11783LinkListFileVersionMajor VersionMajor
        {
            get => _linkListContent.VersionMajor;
            set => _linkListContent.VersionMajor = value;
        }
        public ISO11783LinkListFileVersionMinor VersionMinor
        {
            get => _linkListContent.VersionMinor;
            set => _linkListContent.VersionMinor = value;
        }

        public string ManagementSoftwareManufacturer
        {
            get => _linkListContent.ManagementSoftwareManufacturer;
            set => _linkListContent.ManagementSoftwareManufacturer = value;
        }


        public string ManagementSoftwareVersion
        {
            get => _linkListContent.ManagementSoftwareVersion;
            set => _linkListContent.ManagementSoftwareVersion = value;
        }

        public string TaskControllerManufacturer
        {
            get => _linkListContent.TaskControllerManufacturer;
            set => _linkListContent.TaskControllerManufacturer = value;
        }

        public string TaskControllerVersion
        {
            get => _linkListContent.TaskControllerVersion;
            set => _linkListContent.TaskControllerVersion = value;
        }

        public ISO11783LinkListFileDataTransferOrigin DataTransferOrigin
        {
            get => _linkListContent.DataTransferOrigin;
            set => _linkListContent.DataTransferOrigin = value;
        }

        private IsoLinkList(ISO11783LinkListFile linkListContent)
        {
            if (linkListContent == null)
            {
                _linkListContent = new ISO11783LinkListFile();
                return;
            }
            _linkListContent = linkListContent;
            _groupIds = new IdList("LGP");
            foreach (var group in linkListContent.LinkGroup)
            {
                _groupIds.ReadObject(group);
            }
        }

        internal IsoLinkList()
        {

            _linkListContent = new ISO11783LinkListFile()
            {
                //TODO: Thos data can currently nowhere outside the library be changed. Add functions for that
                DataTransferOrigin = ISO11783LinkListFileDataTransferOrigin.FMIS,
                ManagementSoftwareManufacturer = "unknown",
                ManagementSoftwareVersion = "unknown",
                TaskControllerVersion = "unknown",
                TaskControllerManufacturer = "unknown",
                FileVersion = "1",
                VersionMajor = ISO11783LinkListFileVersionMajor.TheversionofthesecondeditionpublishedasaFinalDraftInternationalStandard,
                VersionMinor = ISO11783LinkListFileVersionMinor.Item3
            };
            _groupIds = new IdList("LGP");
        }


        public string GetID(string uuid)
        {
            foreach (var group in _linkListContent.LinkGroup)
            {
                foreach (var link in group.Link)
                {
                    if (link.LinkValue.Equals(uuid))
                    {
                        return link.ObjectIdRef;
                    }
                }
            }
            return null;
        }

        public string GetFirstLink(string idRef)
        {
            foreach (var group in _linkListContent.LinkGroup)
            {
                foreach (var link in group.Link)
                {
                    if (link.ObjectIdRef.Equals(idRef))
                    {
                        return link.LinkValue;
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// Adds a Link to the Linklist or overwrites a link of the same type
        /// </summary>
        /// <param name="idRef"> An IDRef for an ISOXML Element; e.g. TSK1 or PFD-1</param>
        /// <param name="linkValue">The value of the link, e.g. {d6c6f304-6180-45e2-a9ee-230f408ff897} </param>
        /// <param name="linkToOverwrite">The link to search for that should be overwritten</param>
        /// <param name="type">Default is UUID</param>
        public void AddOrOverWriteLink(string idRef, string linkValue, string linkToOverwrite, ISOLinkGroupType type = ISOLinkGroupType.UUIDs)
        {
            foreach (var group in _linkListContent.LinkGroup)
            {
                if (group.LinkGroupType == type)
                {
                    foreach (var link in group.Link)
                    {
                        if (link.ObjectIdRef.Equals(idRef) && link.LinkValue.Equals(linkToOverwrite))
                        {
                            link.LinkValue = linkValue;
                            return;
                        }
                    }
                    if (group.LinkGroupDesignator.Equals(type.ToString()))
                    {
                        group.Link.Add(new ISOLink()
                        {
                            ObjectIdRef = idRef,
                            LinkValue = linkValue,
                        });
                    }
                }
            }

            var groupToAdd = new ISOLinkGroup()
            {
                LinkGroupDesignator = type.ToString(),
            };
            groupToAdd.Link.Add(new ISOLink()
            {
                ObjectIdRef = idRef,
                LinkValue = linkValue,
            });
            groupToAdd.LinkGroupId = _groupIds.AddObjectAndAssignIdIfNone(groupToAdd);
            _linkListContent.LinkGroup.Add(groupToAdd);
        }

        /// <summary>
        /// Adds a Link to the Linklist if exactly this link does not yet exist
        /// </summary>
        /// <param name="idRef"> An IDRef for an ISOXML Element; e.g. TSK1 or PFD-1</param>
        /// <param name="linkValue">The value of the link, e.g. {d6c6f304-6180-45e2-a9ee-230f408ff897} </param>
        /// <param name="type">Default is UUID</param>
        public void AddLink(string idRef, string linkValue, ISOLinkGroupType type = ISOLinkGroupType.UUIDs)
        {
            foreach (var group in _linkListContent.LinkGroup)
            {
                if (group.LinkGroupType == type)
                {
                    foreach (var link in group.Link)
                    {
                        if (link.ObjectIdRef.Equals(idRef) && link.LinkValue.Equals(linkValue))
                        {
                            return;
                        }

                    }
                    group.Link.Add(new ISOLink()
                    {
                        ObjectIdRef = idRef,
                        LinkValue = linkValue,
                    });
                    return;
                }
            }

            var groupToAdd = new ISOLinkGroup()
            {
                LinkGroupDesignator = "IDs"
            };
            groupToAdd.Link.Add(new ISOLink()
            {
                ObjectIdRef = idRef,
                LinkValue = linkValue,
            });
            groupToAdd.LinkGroupId = _groupIds.AddObjectAndAssignIdIfNone(groupToAdd);
            _linkListContent.LinkGroup.Add(groupToAdd);

        }



        internal static string FixLinkListPath(string path)
        {
            if (path.ToUpper().EndsWith(".XML") == false)
            {
                path = Path.Combine(path.ToString(), "LINKLIST.XML");
            }
            return path;
        }






        internal void SaveLinkList(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path = FixLinkListPath(path);
            LinkListSerializer.Serialize(_linkListContent, path);
        }



        internal static ResultWithMessages<IsoLinkList> ParseLinkList(string isoxmlString, string path)
        {
            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            ResultWithMessages<ISO11783LinkListFile> linkListContent = null;
            var result = new ResultWithMessages<IsoLinkList>();
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(isoxmlString);
                linkListContent = LinkListSerializer.Deserialize(xmlDoc);

            }
            catch (Exception ex)
            {
                result.AddError(
                    ResultMessageCode.XMLParsingError,
                    ResultDetail.FromPath(path),
                    ResultDetail.FromString(ex.Message)
                    );
            }

            var linkList = new IsoLinkList(linkListContent.Result);

            result.AddMessages(linkList._groupIds.CleanListFromTempEntries());
            result.SetResult(linkList);
            return result;
        }

        internal static ResultWithMessages<IsoLinkList> LoadLinkList(string path, string fileName)
        {
            if (!Utils.AdjustFileNameToIgnoreCasing(path, fileName, out var linkListPath))
            {
                var result = new ResultWithMessages<IsoLinkList>();
                result.AddError(
                    ResultMessageCode.FileNotFound,
                    ResultDetail.FromFile(fileName)
                    );
                result.SetResult(new IsoLinkList());
                return result;
            }
            else
            {
                var text = File.ReadAllText(linkListPath.ToString());
                return ParseLinkList(text, path);
            }

        }


    }
}
