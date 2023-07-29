using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using Dev4Agriculture.ISO11783.ISOXML.Geometry;
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

    [TestMethod]
    public void CalculatingIntersectArea_SimilarFieldWithMoreDots()
    {
        var filePath = "./testdata/LoadFromStream/MultiFields.zip";
        ISOXML isoxml = null;
        using (var stream = File.OpenRead(filePath))
        {
            isoxml = ISOXML.LoadFromArchive(stream);
        }
        var fields = isoxml.Data.Partfield.ToList();
        var result = new IntersectionResult { IntersectPercent = 0.747, Type = IntersectionAlgorithmType.WeightCenter, PolygonType = PolygonType.Concave };

        var actualResult = fields[1].TryGetOverlapWithPartfield(fields[9]);

        Assert.AreEqual(result.IntersectPercent, Math.Round(actualResult.IntersectPercent, 3));
        Assert.AreEqual(result.Type, actualResult.Type);
        Assert.AreEqual(result.PolygonType, actualResult.PolygonType);
    }


    [TestMethod]
    public void CalculatingIntersectArea_HalfIntersection()
    {
        var filePath = "./testdata/LoadFromStream/MultiFields.zip";
        ISOXML isoxml = null;
        using (var stream = File.OpenRead(filePath))
        {
            isoxml = ISOXML.LoadFromArchive(stream);
        }
        var fields = isoxml.Data.Partfield.ToList();
        var result = new IntersectionResult { IntersectPercent = 0.58, Type = IntersectionAlgorithmType.WeightCenter, PolygonType = PolygonType.Convex  };

        var actualResult = fields[7].TryGetOverlapWithPartfield(fields[8]);

        Assert.AreEqual(result.IntersectPercent, Math.Round(actualResult.IntersectPercent, 2));
        Assert.AreEqual(result.Type, actualResult.Type);
        Assert.AreEqual(result.PolygonType, actualResult.PolygonType);
    }

    [TestMethod]
    public void CalculatingIntersectArea_FieldHasOtherFieldInside()
    {
        var filePath = "./testdata/LoadFromStream/MultiFields.zip";
        ISOXML isoxml = null;
        using (var stream = File.OpenRead(filePath))
        {
            isoxml = ISOXML.LoadFromArchive(stream);
        }
        var fields = isoxml.Data.Partfield.ToList();
        var result = new IntersectionResult { IntersectPercent = 0.1235, Type = IntersectionAlgorithmType.WeightCenter, PolygonType = PolygonType.Convex };

        var actualResult = fields[3].TryGetOverlapWithPartfield(fields[4]);

        Assert.AreEqual(result.IntersectPercent, Math.Round(actualResult.IntersectPercent, 4));
        Assert.AreEqual(result.Type, actualResult.Type);
        Assert.AreEqual(result.PolygonType, actualResult.PolygonType);
    }

    [TestMethod]
    public void CalculatingIntersectArea_FieldInsideOther()
    {
        var filePath = "./testdata/LoadFromStream/MultiFields.zip";
        ISOXML isoxml = null;
        using (var stream = File.OpenRead(filePath))
        {
            isoxml = ISOXML.LoadFromArchive(stream);
        }
        var fields = isoxml.Data.Partfield.ToList();
        var result = new IntersectionResult { IntersectPercent = 1.8746, Type = IntersectionAlgorithmType.WeightCenterReversed, PolygonType = PolygonType.Convex };

        var actualResult = fields[4].TryGetOverlapWithPartfield(fields[3]);

        Assert.AreEqual(result.IntersectPercent, Math.Round(actualResult.IntersectPercent, 4));
        Assert.AreEqual(result.Type, actualResult.Type);
        Assert.AreEqual(result.PolygonType, actualResult.PolygonType);
    }

    [TestMethod]
    public void CalculatingIntersectArea_SameField()
    {
        var filePath = "./testdata/LoadFromStream/MultiFields.zip";
        ISOXML isoxml = null;
        using (var stream = File.OpenRead(filePath))
        {
            isoxml = ISOXML.LoadFromArchive(stream);
        }
        var fields = isoxml.Data.Partfield.ToList();
        var result = new IntersectionResult { IntersectPercent = 1, Type = IntersectionAlgorithmType.Bounds, PolygonType = PolygonType.None };

        var actualResult = fields[4].TryGetOverlapWithPartfield(fields[4]);

        Assert.AreEqual(result.IntersectPercent, actualResult.IntersectPercent);
        Assert.AreEqual(result.Type, actualResult.Type);
        Assert.AreEqual(result.PolygonType, actualResult.PolygonType);
    }


    [TestMethod]
    public void CanCheckOverlapOfBounds()
    {
        var filePath = "./testdata/LoadFromStream/MultiFields.zip";
        ISOXML isoxml = null;
        using (var stream = File.OpenRead(filePath))
        {
            isoxml = ISOXML.LoadFromArchive(stream);
        }
        var fields = isoxml.Data.Partfield.ToList();
        var AustraliaBounds = new FieldBounds()
        {
            MaxLat = -20,
            MinLat = -40,
            MaxLong = -60,
            MinLong = -120
        };
        var GermanyBounds = new FieldBounds()
        {
            MaxLat = 49,
            MinLat = 48,
            MaxLong = 11.8m,
            MinLong = 11
        };
        foreach( var field in fields)
        {
            var result = field.IsOverlappingBounds(AustraliaBounds);
            Assert.IsFalse(result);
        }
        foreach (var field in fields)
        {
            var result = field.IsOverlappingBounds(GermanyBounds);
            Assert.IsTrue(result);
        }

    }

}
