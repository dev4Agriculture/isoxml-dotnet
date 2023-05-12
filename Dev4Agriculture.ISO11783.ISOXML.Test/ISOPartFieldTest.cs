using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

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
                    new ISOPoint{PointNorth = 48.079711m,PointEast = 11.611686m},
                    new ISOPoint{PointNorth = 48.081546m,PointEast = 11.611879m},
                    new ISOPoint{PointNorth = 48.081431m,PointEast = 11.617136m},
                    new ISOPoint{PointNorth = 48.076829m,PointEast = 11.620419m},
                    new ISOPoint{PointNorth = 48.075711m,PointEast = 11.614776m},
                }
            }}
        });
        var res= field.CalculateArea();
        var ratio = res / refValue;
        Assert.IsTrue(ratio > 0.99 && ratio < 1.01);
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
                    new ISOPoint{PointNorth = 48.079711m,PointEast = 11.611686m},
                    new ISOPoint{PointNorth = 48.081546m,PointEast = 11.611879m},
                    new ISOPoint{PointNorth = 48.081431m,PointEast = 11.617136m},
                    new ISOPoint{PointNorth = 48.076829m,PointEast = 11.620419m},
                    new ISOPoint{PointNorth = 48.075711m,PointEast = 11.614776m},
                    new ISOPoint{PointNorth = 48.079711m,PointEast = 11.611686m},
                }
            }}
        });
        var res= field.CalculateArea();
        var ratio = res / refValue;
        Assert.IsTrue(ratio > 0.99 && ratio < 1.01);
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
            Assert.IsTrue(ratio > 0.99 && ratio < 1.01);
        }
    }

}
