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
        internal static string fixLinkListPath(string path)
        {
            if (path.ToUpper().EndsWith(".XML") == false)
            {
                path = Path.Combine(path.ToString(), "LINKLIST.xml");
            }
            return path;
        }


        public static ResultWithMessages<ISO11783LinkListFile> ParseLinkList(string isoxmlString, string path)
        {
            ISO11783LinkListFile linkList = null;
            var messages = new List<ResultMessage>();
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(isoxmlString);
                linkList = (ISO11783LinkListFile)linkListSerializer.Deserialize(xmlDoc);

                messages.AddRange(linkListSerializer.messages);
            }
            catch (Exception ex)
            {
                messages.Add(new ResultMessage(ResultMessageType.Error, ex.Message));
            }
            return new ResultWithMessages<ISO11783LinkListFile>(linkList, messages);
        }

        public static ResultWithMessages<ISO11783LinkListFile> LoadLinkList(string path)
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


        public static void SaveLinkList(ISO11783LinkListFile linkList, string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path = fixLinkListPath(path);
            IsoLinkList.linkListSerializer.Serialize(linkList, path);
        }

    }
}