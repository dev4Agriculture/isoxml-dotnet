using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dev4ag
{
    [TestClass]
    public class WSMTest
    {

        [TestMethod]
        public void canReadValidWSM()
        {
            string clientName = "A00E84000DE0DB4B";//This mostly comes from a Device.D Attribute (Device.ClientName)
            WSM wsm = new WSM(clientName);
            Assert.AreEqual(wsm.manufacturerCode, 111);
            Assert.AreEqual(wsm.deviceClass, DeviceClass.Harvesters);
        }
    }
}
