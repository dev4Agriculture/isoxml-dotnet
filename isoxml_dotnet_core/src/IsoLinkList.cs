using Dev4ag.ISO11783.LinkListFile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Dev4ag
{


    public class IsoLinkList
    {
        private static LinkListSerializer linkListSerializer = new LinkListSerializer();
        
        private readonly ISO11783LinkListFile LinkListContent;
        private IDList GroupIds;



        internal IsoLinkList(ISO11783LinkListFile linkListContent)
        {
            if( linkListContent == null)
            {
                this.LinkListContent = new ISO11783LinkListFile();
                return;
            }
            this.LinkListContent = linkListContent;
            this.GroupIds = new IDList("LGP");
            foreach(var group in linkListContent.LinkGroup)
            {
                this.GroupIds.AddObject(group);
            }
        }

        internal IsoLinkList()
        {

            this.LinkListContent = new ISO11783LinkListFile()
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
            this.GroupIds = new IDList("LGP");
        }


        public string GetID(string uuid)
        {
            foreach(var group in this.LinkListContent.LinkGroup)
            {
                foreach(var link in group.Link)
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
            foreach (var group in this.LinkListContent.LinkGroup)
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
        public void AddOrOverWriteLink(string idRef, string linkValue, string linkToOverwrite, LinkGroupType type = LinkGroupType.UUIDs)
        {
            foreach (var group in this.LinkListContent.LinkGroup)
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
                        group.Link.Add(new Link()
                        {
                            ObjectIdRef = idRef,
                            LinkValue = linkValue,
                        });
                    }
                }
            }

            LinkGroup groupToAdd = new LinkGroup()
            {
                LinkGroupDesignator = type.ToString(),
            };
            groupToAdd.Link.Add(new Link()
            {
                ObjectIdRef = idRef,
                LinkValue = linkValue,
            });
            groupToAdd.LinkGroupId = GroupIds.AddObject(groupToAdd);
            LinkListContent.LinkGroup.Add(groupToAdd);
        }

        /// <summary>
        /// Adds a Link to the Linklist if exactly this link does not yet exist
        /// </summary>
        /// <param name="idRef"> An IDRef for an ISOXML Element; e.g. TSK1 or PFD-1</param>
        /// <param name="linkValue">The value of the link, e.g. {d6c6f304-6180-45e2-a9ee-230f408ff897} </param>
        /// <param name="type">Default is UUID</param>
        public void AddLink(string idRef, string linkValue, LinkGroupType type = LinkGroupType.UUIDs)
        {
            foreach (var group in this.LinkListContent.LinkGroup)
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
                    group.Link.Add(new Link()
                    {
                        ObjectIdRef = idRef,
                        LinkValue = linkValue,
                    });
                    return;
                }
            }

            LinkGroup groupToAdd = new LinkGroup()
            {
                LinkGroupDesignator = "IDs"
            };
            groupToAdd.Link.Add(new Link()
            {
                ObjectIdRef = idRef,
                LinkValue = linkValue,
            });
            groupToAdd.LinkGroupId =  GroupIds.AddObject(groupToAdd);
            LinkListContent.LinkGroup.Add(groupToAdd);

        }



        internal static string fixLinkListPath(string path)
        {
            if (path.ToUpper().EndsWith(".XML") == false)
            {
                path = Path.Combine(path.ToString(), "LINKLIST.xml");
            }
            return path;
        }






        internal void SaveLinkList(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path = fixLinkListPath(path);
            IsoLinkList.linkListSerializer.Serialize(LinkListContent, path);
        }



        internal static ResultWithMessages<IsoLinkList> ParseLinkList(string isoxmlString, string path)
        {
            ISO11783LinkListFile linkListContent = null;
            var messages = new List<ResultMessage>();
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(isoxmlString);
                linkListContent = (ISO11783LinkListFile)linkListSerializer.Deserialize(xmlDoc);

                messages.AddRange(linkListSerializer.messages);
            }
            catch (Exception ex)
            {
                messages.Add(new ResultMessage(ResultMessageType.Error, ex.Message));
            }

            var linkList = new IsoLinkList(linkListContent);

            return new ResultWithMessages<IsoLinkList>(linkList, messages);
        }

        internal static ResultWithMessages<IsoLinkList> LoadLinkList(string path)
        {
            var messages = new List<ResultMessage>();
            path = fixLinkListPath(path);
            if (File.Exists(path) == false)
            {
                messages.Add(new ResultMessage(ResultMessageType.Error, "LINKLIST.xml not found!"));
            }
            string text = File.ReadAllText(path.ToString());
            var result = ParseLinkList(text, path);
            return result;
        }


    }
}