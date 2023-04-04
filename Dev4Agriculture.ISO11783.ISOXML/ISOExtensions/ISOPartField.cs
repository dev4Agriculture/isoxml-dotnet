using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

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

        private FieldBounds UpdateFromPoints(FieldBounds bounds, IEnumerable<ISOPoint> points)
        {
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



        public bool IsInField(decimal longitude, decimal latitude)
        {
            return PolygonnonTreatmentZoneonly.First().IsInPolygon(longitude, latitude);
        }
    }
}
