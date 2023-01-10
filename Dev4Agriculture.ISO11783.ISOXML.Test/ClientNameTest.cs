using Dev4Agriculture.ISO11783.ISOXML.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev4Agriculture.ISO11783.ISOXML.Test;

[TestClass]
public class ClientNameTest
{

    [TestMethod]
    public void CanReadValidClientName()
    {
        var clientNameString = "A00E84000DE0DB4B";//This mostly comes from a Device.D Attribute (Device.ClientName)
        var clientName = new ClientName(clientNameString);
        Assert.AreEqual(clientName.ManufacturerCode, 111);
        Assert.AreEqual(clientName.DeviceClass, DeviceClass.Harvesters);
    }


    [TestMethod]
    public void CanReadValidLowerCaseClientName()
    {
        var clientNameString = "a00e84000de0db4b";//This mostly comes from a Device.D Attribute (Device.ClientName)
        var clientName = new ClientName(clientNameString);
        Assert.AreEqual(clientName.ManufacturerCode, 111);
        Assert.AreEqual(clientName.DeviceClass, DeviceClass.Harvesters);
    }


    [TestMethod]
    public void CanThrowErrors()
    {
        Assert.ThrowsException<ClientNameTooLongException>(() => new ClientName("abcdefdafdaffaefafeaefafaffaefaefef"));
        Assert.ThrowsException<ClientNameTooShortException>(() => new ClientName("abcd"));
    }

}
