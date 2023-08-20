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
    public void CanRecognizeClosedPolygonFieldAsEqualToNonClosedPolygon()
    {
        var pfd1 = new ISOPartfield();
        var pln1 = new ISOPolygon()
        {
            PolygonType = ISOPolygonType.PartfieldBoundary
        };
        var lsg1 = new ISOLineString()
        {
            LineStringType = ISOLineStringType.PolygonExterior
        };
        lsg1.Point.Add(new ISOPoint() { PointNorth = (decimal)52.3154258728, PointEast = (decimal)7.5933585167 });
        lsg1.Point.Add(new ISOPoint() { PointNorth = (decimal)52.3155326843, PointEast = (decimal)7.5938162804 });
        lsg1.Point.Add(new ISOPoint() { PointNorth = (decimal)52.3157234192, PointEast = (decimal)7.5947408676 });
        lsg1.Point.Add(new ISOPoint() { PointNorth = (decimal)52.3159179688, PointEast = (decimal)7.5956292152 });
        lsg1.Point.Add(new ISOPoint() { PointNorth = (decimal)52.3160095215, PointEast = (decimal)7.596060276 });
        lsg1.Point.Add(new ISOPoint() { PointNorth = (decimal)52.3162765503, PointEast = (decimal)7.5960063934 });
        lsg1.Point.Add(new ISOPoint() { PointNorth = (decimal)52.3163757324, PointEast = (decimal)7.5957460403 });
        lsg1.Point.Add(new ISOPoint() { PointNorth = (decimal)52.3163833618, PointEast = (decimal)7.5955486298 });
        lsg1.Point.Add(new ISOPoint() { PointNorth = (decimal)52.3164215088, PointEast = (decimal)7.5950818062 });
        lsg1.Point.Add(new ISOPoint() { PointNorth = (decimal)52.3164558411, PointEast = (decimal)7.5933494568 });
        lsg1.Point.Add(new ISOPoint() { PointNorth = (decimal)52.3164634705, PointEast = (decimal)7.592900753 });
        lsg1.Point.Add(new ISOPoint() { PointNorth = (decimal)52.3160705566, PointEast = (decimal)7.593026638 });
        lsg1.Point.Add(new ISOPoint() { PointNorth = (decimal)52.3155174255, PointEast = (decimal)7.5932326317 });
        pln1.LineString.Add(lsg1);
        pfd1.PolygonnonTreatmentZoneonly.Add(pln1);


        var pfd2 = new ISOPartfield();
        var pln2 = new ISOPolygon()
        {
            PolygonType = ISOPolygonType.PartfieldBoundary
        };
        var lsg2 = new ISOLineString()
        {
            LineStringType = ISOLineStringType.PolygonExterior
        };

        lsg2.Point.Add(new ISOPoint() { PointNorth = (decimal)52.3154258728, PointEast = (decimal)7.5933585167 });
        lsg2.Point.Add(new ISOPoint() { PointNorth = (decimal)52.3155326843, PointEast = (decimal)7.5938162804 });
        lsg2.Point.Add(new ISOPoint() { PointNorth = (decimal)52.3157234192, PointEast = (decimal)7.5947408676 });
        lsg2.Point.Add(new ISOPoint() { PointNorth = (decimal)52.3159179688, PointEast = (decimal)7.5956292152 });
        lsg2.Point.Add(new ISOPoint() { PointNorth = (decimal)52.3160095215, PointEast = (decimal)7.596060276 });
        lsg2.Point.Add(new ISOPoint() { PointNorth = (decimal)52.3162765503, PointEast = (decimal)7.5960063934 });
        lsg2.Point.Add(new ISOPoint() { PointNorth = (decimal)52.3163757324, PointEast = (decimal)7.5957460403 });
        lsg2.Point.Add(new ISOPoint() { PointNorth = (decimal)52.3163833618, PointEast = (decimal)7.5955486298 });
        lsg2.Point.Add(new ISOPoint() { PointNorth = (decimal)52.3164215088, PointEast = (decimal)7.5950818062 });
        lsg2.Point.Add(new ISOPoint() { PointNorth = (decimal)52.3164558411, PointEast = (decimal)7.5933494568 });
        lsg2.Point.Add(new ISOPoint() { PointNorth = (decimal)52.3164634705, PointEast = (decimal)7.592900753 });
        lsg2.Point.Add(new ISOPoint() { PointNorth = (decimal)52.3160705566, PointEast = (decimal)7.593026638 });
        lsg2.Point.Add(new ISOPoint() { PointNorth = (decimal)52.3155174255, PointEast = (decimal)7.5932326317 });
        lsg2.Point.Add(new ISOPoint() { PointNorth = (decimal)52.3154258728, PointEast = (decimal)7.5933585167 });
        pln2.LineString.Add(lsg2);
        pfd2.PolygonnonTreatmentZoneonly.Add(pln2);

        var result = pfd2.TryGetOverlapWithPartfield(pfd1);
        Assert.IsTrue(result.Type == IntersectionAlgorithmType.Bounds);
        Assert.AreEqual(1, result.IntersectPercent);

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
