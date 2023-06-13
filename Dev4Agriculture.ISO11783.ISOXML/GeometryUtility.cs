using System;
using System.Collections.Generic;
using System.Linq;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;

namespace Dev4Agriculture.ISO11783.ISOXML
{
    public static class GeometryUtility
    {
        private const decimal EquityTolerance = 0.000000001m;

        private static List<ISOPoint> Order(this List<ISOPoint> points)
        {
            if (!points.Any())
                return points;
            decimal mX = 0;
            decimal my = 0;
            foreach (var p in points)
            {
                mX += p.PointEast;
                my += p.PointNorth;
            }
            mX /= points.Count();
            my /= points.Count();
            return points.OrderBy(v => Math.Atan2((double)(v.PointNorth - my), (double)(v.PointEast - mX))).ToList();
        }

        public static List<ISOPoint> GetIntersectionOfPolygons(this List<ISOPoint> poly1, List<ISOPoint> poly2)
        {
            var clippedCorners = new List<ISOPoint>{};
            //Add  the corners of poly1 which are inside poly2       
            foreach (var t in poly1.Where(t => IsPointInsidePoly(t, poly2)))
            {
                AddPoints(clippedCorners, new[] { t });
            }

            //Add the corners of poly2 which are inside poly1
            foreach (var t in poly2.Where(t => IsPointInsidePoly(t, poly1)))
            {
                AddPoints(clippedCorners, new[] { t });
            }

            //Add  the intersection points
            for (int i = 0, next = 1; i < poly1.Count(); i++, next = (i + 1 == poly1.Count()) ? 0 : i + 1)
            {
                AddPoints(clippedCorners, GetIntersectionPoints(poly1[i], poly1[next], poly2));
            }

            var res = new List<ISOPoint>();
            res.AddRange(clippedCorners.Order());
            return res;
        }

        private static bool IsEqual(decimal d1, decimal d2) => Math.Abs(d1 - d2) <= EquityTolerance;

        private static bool IsPointInsidePoly(ISOPoint test, List<ISOPoint> poly)
        {
            int i;
            int j;
            var result = false;
            for (i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            {
                if ((poly[i].PointNorth > test.PointNorth) != (poly[j].PointNorth > test.PointNorth) &&
                    (test.PointEast < (poly[j].PointEast - poly[i].PointEast) * (test.PointNorth - poly[i].PointNorth) / (poly[j].PointNorth - poly[i].PointNorth) + poly[i].PointEast))
                {
                    result = !result;
                }
            }
            return result;
        }

        private static void AddPoints(List<ISOPoint> pool, ISOPoint[] newpoints)
        {
            foreach (var np in newpoints)
            {
                var found = false;
                foreach (var p in pool)
                {
                    if (IsEqual(p.PointEast, np.PointEast) && IsEqual(p.PointNorth, np.PointNorth))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    pool.Add(np);
                }
            }
        }

        private static ISOPoint GetIntersectionPoint(ISOPoint l1P1, ISOPoint l1P2, ISOPoint l2P1, ISOPoint l2P2)
        {
            var a1 = l1P2.PointNorth - l1P1.PointNorth;
            var b1 = l1P1.PointEast - l1P2.PointEast;
            var c1 = a1 * l1P1.PointEast + b1 * l1P1.PointNorth;
            var a2 = l2P2.PointNorth - l2P1.PointNorth;
            var b2 = l2P1.PointEast - l2P2.PointEast;
            var c2 = a2 * l2P1.PointEast + b2 * l2P1.PointNorth;
            //lines are parallel
            var det = a1 * b2 - a2 * b1;
            if (IsEqual(det, 0m))
                return null;

            var x = (b2 * c1 - b1 * c2) / det;
            var y = (a1 * c2 - a2 * c1) / det;
            var online1 = (Math.Min(l1P1.PointEast, l1P2.PointEast) < x || IsEqual(Math.Min(l1P1.PointEast, l1P2.PointEast), x))
                           && (Math.Max(l1P1.PointEast, l1P2.PointEast) > x || IsEqual(Math.Max(l1P1.PointEast, l1P2.PointEast), x))
                           && (Math.Min(l1P1.PointNorth, l1P2.PointNorth) < y || IsEqual(Math.Min(l1P1.PointNorth, l1P2.PointNorth), y))
                           && (Math.Max(l1P1.PointNorth, l1P2.PointNorth) > y || IsEqual(Math.Max(l1P1.PointNorth, l1P2.PointNorth), y))
                ;
            var online2 = (Math.Min(l2P1.PointEast, l2P2.PointEast) < x || IsEqual(Math.Min(l2P1.PointEast, l2P2.PointEast), x))
                           && (Math.Max(l2P1.PointEast, l2P2.PointEast) > x || IsEqual(Math.Max(l2P1.PointEast, l2P2.PointEast), x))
                           && (Math.Min(l2P1.PointNorth, l2P2.PointNorth) < y || IsEqual(Math.Min(l2P1.PointNorth, l2P2.PointNorth), y))
                           && (Math.Max(l2P1.PointNorth, l2P2.PointNorth) > y || IsEqual(Math.Max(l2P1.PointNorth, l2P2.PointNorth), y))
                ;
            if (online1 && online2)
                return new ISOPoint{PointEast = x, PointNorth = y};
            return null;
        }

        private static ISOPoint[] GetIntersectionPoints(ISOPoint l1P1, ISOPoint l1P2, List<ISOPoint> poly)
        {
            var intersectionPoints = new List<ISOPoint>();
            for (var i = 0; i < poly.Count; i++)
            {
                var next = (i + 1 == poly.Count) ? 0 : i + 1;
                var ip = GetIntersectionPoint(l1P1, l1P2, poly[i], poly[next]);
                if (ip != null) intersectionPoints.Add(ip);
            }
            return intersectionPoints.ToArray();
        }
    }
}
