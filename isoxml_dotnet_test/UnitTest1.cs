using de.dev4agriculture.core.iso11783.taskdata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Xml.Serialization;

namespace isoxml_dotnet_test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void LoadSimpleTaskData()
        {
            //var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Names.txt"); +"/testdata/";
            XmlSerializer serializer = new XmlSerializer(typeof(ISO11783_TaskData));
            string text = File.ReadAllText("./testdata/devices/TASKDATA_Devices.XML");
            using ( StringReader reader = new StringReader(text))
            {
                var output = (ISO11783_TaskData)serializer.Deserialize(reader);
                var subElements = output.Items;
                foreach(var codingData in subElements)
                {
                    if( codingData is TSK)
                    {
                        Console.WriteLine("Task Found");
                    } else if ( codingData is DVC)
                    {
                        Console.WriteLine("Device Found");
                    }
                }
            }
        }
    }
}
