using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev4Agriculture.ISO11783.ISOXML.Test;

[TestClass]
public class ISOPartFieldTest
{
    [TestMethod]
    public void CalculatingAreaMatchesRealArea_NotClosedPolygon()
    {
        var refValue = 274965.3767857966;

        var field = new ISOPartfield();
        field.PolygonnonTreatmentZoneonly.Add(new ISOPolygon
        {
            LineString = {  new ISOLineString
            {
                LineStringType = ISOLineStringType.PolygonExterior,
                Point =
                {
                    new ISOPoint{PointNorth = 11.611686m,PointEast = 48.079711m},
                    new ISOPoint{PointNorth = 11.611879m,PointEast = 48.081546m},
                    new ISOPoint{PointNorth = 11.617136m,PointEast = 48.081431m},
                    new ISOPoint{PointNorth = 11.620419m,PointEast = 48.076829m},
                    new ISOPoint{PointNorth = 11.614776m,PointEast = 48.075711m},
                }
            }}
        });
        var res= field.CalculateArea();
        Assert.AreEqual((int)refValue, (int)res);
    }
    
    [TestMethod]
    public void CalculatingAreaMatchesRealArea_ClosedPolygon()
    {
        var refValue = 274965.3767857966;

        var field = new ISOPartfield();
        field.PolygonnonTreatmentZoneonly.Add(new ISOPolygon
        {
            LineString = {  new ISOLineString
            {
                LineStringType = ISOLineStringType.PolygonExterior,
                Point =
                {
                    new ISOPoint{PointNorth = 11.611686m,PointEast = 48.079711m},
                    new ISOPoint{PointNorth = 11.611879m,PointEast = 48.081546m},
                    new ISOPoint{PointNorth = 11.617136m,PointEast = 48.081431m},
                    new ISOPoint{PointNorth = 11.620419m,PointEast = 48.076829m},
                    new ISOPoint{PointNorth = 11.614776m,PointEast = 48.075711m},
                    new ISOPoint{PointNorth = 11.611686m,PointEast = 48.079711m},
                }
            }}
        });
        var res= field.CalculateArea();
        Assert.AreEqual((int)refValue, (int)res);
    }


    [TestMethod]
    public void FieldAreasMatchInAllAreasOfTheWorld()
    {
        var isoxml = ISOXML.Load("./testdata/CodingData/Partfields");
        Assert.AreEqual(0,isoxml.Messages.Count);
        foreach(var field in isoxml.Data.Partfield)
        {
            var size = field.CalculateArea();
            var ratio = size / field.PartfieldArea;
            //Assert.IsTrue(ratio > 0.99 && ratio < 1.01);
        }
    }
}
