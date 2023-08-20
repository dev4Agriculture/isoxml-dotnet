using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Dev4Agriculture.ISO11783.ISOXML.Geometry;

namespace Dev4Agriculture.ISO11783.ISOXML.TaskFile
{
    public class FieldBounds
    {
        public decimal MinLat = decimal.MaxValue;
        public decimal MinLong = decimal.MaxValue;
        public decimal MaxLat = decimal.MinValue;
        public decimal MaxLong = decimal.MinValue;

        public void Update(decimal north, decimal east)
        {
            MinLat = MinLat < north ? MinLat : north;
            MinLong = MinLong < east ? MinLong : east;
            MaxLat = MaxLat > north ? MaxLat : north;
            MaxLong = MaxLong > east ? MaxLong : east;
        }
    }

    public partial class ISOPartfield
    {
        [XmlIgnore]
        public FieldBounds Bounds;

        private FieldBounds UpdateFromPoints(FieldBounds bounds, IEnumerable<ISOPoint> points) {
            foreach (var point in points)
            {
                bounds.Update(point.PointNorth, point.PointEast);
            }
            return bounds;
        }

        private FieldBounds UpdateFromLineStrings(FieldBounds bounds, IEnumerable<ISOLineString> lineStrings)
        {
            foreach (var lineString in lineStrings)
            {
                bounds = UpdateFromPoints(bounds, lineString.Point);
            }
            return bounds;

        }

        public FieldBounds CalculateBounds()
        {
            var bounds = new FieldBounds();
            foreach (var polygon in PolygonnonTreatmentZoneonly)
            {
                bounds = UpdateFromLineStrings(bounds, polygon.LineString);
            }
            bounds = UpdateFromLineStrings(bounds, LineString);
            bounds = UpdateFromPoints(bounds, Point);

            return bounds;

        }

        public bool IsInField(decimal longitude,decimal latitude)
        {
            return PolygonnonTreatmentZoneonly.First().IsInPolygon(longitude, latitude);
        }

        public bool IsOverlappingBounds(FieldBounds bounds)
        {
            Bounds ??= CalculateBounds();

            return !(
                Bounds.MaxLong < bounds.MinLong ||
                Bounds.MinLong > bounds.MaxLong ||
                Bounds.MaxLat < bounds.MinLat ||
                Bounds.MinLat > bounds.MaxLat);

        }


        /// <summary>
        /// Returns algorithm type, polygon type and intersect percent with secondField
        /// </summary>
        /// <param name="secondField">The ISOPartfield</param>
        /// <returns>IntersectionResult model</returns>
        public IntersectionResult TryGetOverlapWithPartfield(ISOPartfield secondField)
        {
            var result = new IntersectionResult();
            var polygon = PolygonnonTreatmentZoneonly.FirstOrDefault(s => s.PolygonType == ISOPolygonType.PartfieldBoundary);
            if (polygon == null)
                return null;
            var exterior = polygon.LineString.FirstOrDefault(s => s.LineStringType == ISOLineStringType.PolygonExterior);
            if (exterior == null)
                return null;
            var polygonSecond = secondField.PolygonnonTreatmentZoneonly.FirstOrDefault(s => s.PolygonType == ISOPolygonType.PartfieldBoundary);
            if (polygonSecond == null)
                return null;
            var exteriorSecond = polygonSecond.LineString.FirstOrDefault(s => s.LineStringType == ISOLineStringType.PolygonExterior);
            if (exteriorSecond == null)
                return null;

            var p1 = exterior.Point.ToList();
            var p2 = exteriorSecond.Point.ToList();
            if (p1[0].PointNorth == p1[^1].PointNorth && p1[0].PointEast == p1[^1].PointEast)
                p1.RemoveAt(p1.Count - 1);
            if (p2[0].PointNorth == p2[^1].PointNorth && p2[0].PointEast == p2[^1].PointEast)
                p2.RemoveAt(p2.Count - 1);

            p1 = GeometryUtility.ClearPolygon(p1);
            p2 = GeometryUtility.ClearPolygon(p2);

            if (GeometryUtility.ArePointsEqual(p1, p2))
            {
                result.IntersectPercent = 1;
                result.PolygonType = PolygonType.None;
                result.Type = IntersectionAlgorithmType.Bounds;
                return result;
            }
            var intersectedArea = p1.GetIntersectionOfConvexPolygons(p2);
            if (!intersectedArea.Any())
            {
                return null;
            }
            result.PolygonType = p2.PolygonIsConvex() ? PolygonType.Convex : PolygonType.Concave;

            if (GeometryUtility.ArePointsEqual(p1, intersectedArea))
            {
                var p2Area = GetArea(p2.ToArray());
                var reverseIntersectedArea = p2.GetIntersectionOfConvexPolygons(p1);
                var reverseArea = GetArea(reverseIntersectedArea.ToArray());
                //2 is two times area of bigger polygon 
                result.IntersectPercent = 2 - reverseArea / p2Area;
                result.Type = IntersectionAlgorithmType.WeightCenterReversed;
                return result;
            }

            var baseArea = GetArea(exterior.Point.ToArray());
            var intersectArea = GetArea(intersectedArea.ToArray());
            result.IntersectPercent = intersectArea / baseArea;
            result.Type = IntersectionAlgorithmType.WeightCenter;
            return result;
        }

        /// <summary>
        ///  Calculate area for field
        /// </summary>
        /// <returns>The Area in m^2</returns>
        public double CalculateArea()
        {
            var polygon = PolygonnonTreatmentZoneonly.FirstOrDefault(s => s.PolygonType == ISOPolygonType.PartfieldBoundary);
            if (polygon == null)
            {
                return 0;
            }

            var exterior = polygon.LineString.FirstOrDefault(s => s.LineStringType == ISOLineStringType.PolygonExterior);
            if (exterior == null)
            {
                return 0;
            }

            var area = GetArea(exterior.Point.ToArray());

            var interiorPolygons = polygon.LineString.Where(s => s.LineStringType == ISOLineStringType.PolygonInterior);
            if (interiorPolygons.Any())
            {
                foreach (var interior in interiorPolygons)
                {
                    area -= GetArea(interior.Point.ToArray());
                }
            }
            return area;
        }

        private double GetArea(ISOPoint[] data) => ComputeArea(data);

        private static double ComputeArea(ISOPoint[] path) => Math.Abs(ComputeSignedArea(path, Constants.EarthRadiusInMeters));

        private static double ToRadians(decimal input) => (double)input / 180.0 * Math.PI;

        private static double ComputeSignedArea(ISOPoint[] path, double radius)
        {
            var size = path.Length;
            if (size < 3)
            {
                return 0;
            }
            double total = 0;
            var prev = path[size - 1];
            var prevTanLat = Math.Tan((Math.PI / 2 - ToRadians(prev.PointNorth)) / 2);
            var prevLng = ToRadians(prev.PointEast);

            foreach (var point in path)
            {
                var tanLat = Math.Tan((Math.PI / 2 - ToRadians(point.PointNorth)) / 2);
                var lng = ToRadians(point.PointEast);
                total += PolarTriangleArea(tanLat, lng, prevTanLat, prevLng);
                prevTanLat = tanLat;
                prevLng = lng;
            }
            return total * (radius * radius);
        }

        private static double PolarTriangleArea(double tan1, double lng1, double tan2, double lng2)
        {
            var deltaLng = lng1 - lng2;
            var t = tan1 * tan2;
            return 2 * Math.Atan2(t * Math.Sin(deltaLng), 1 + t * Math.Cos(deltaLng));
        }

        internal void FixPositionDigits()
        {
            foreach (var pnt in Point)
            {
                pnt.FixDigits();
            }
            foreach (var lsg in LineString)
            {
                lsg.FixPointDigits();
            }
            foreach (var pln in PolygonnonTreatmentZoneonly)
            {
                pln.FixPointDigits();
            }



            foreach (var ggp in GuidanceGroup)
            {
                foreach (var gpn in ggp.GuidancePattern)
                {
                    foreach (var lsg in gpn.LineString)
                    {
                        lsg.FixPointDigits();
                    }

                    foreach (var pln in gpn.BoundaryPolygon)
                    {
                        pln.FixPointDigits();
                    }
                }
            }
        }
    }
}
