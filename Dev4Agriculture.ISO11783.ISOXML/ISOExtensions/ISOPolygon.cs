﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev4Agriculture.ISO11783.ISOXML.TaskFile
{
    public partial class ISOPolygon
    {
        

        private bool IsInPolygonLineString(decimal longitude, decimal latitude, ISOLineString lineString)
        {
            var points = lineString.Point;
            // Get the angle between the point and the
            // first and last vertices.
            int max_point = points.Count - 1;
            var total_angle = MathUtils.GetAngle(
                (float)points[max_point].PointEast, (float)points[max_point].PointNorth,
                (float)longitude, (float)latitude,
                (float)points[0].PointEast, (float)points[0].PointNorth
                );

            // Add the angles from the point
            // to each other pair of vertices.
            for (int i = 0; i < max_point; i++)
            {
                total_angle += MathUtils.GetAngle(
                     (float)points[i].PointEast, (float)points[i].PointNorth,
                     (float)longitude, (float)latitude,
                     (float)points[i + 1].PointEast, (float)points[i + 1].PointNorth);
            };

            // The total angle should be 2 * PI or -2 * PI if
            // the point is in the polygon and close to zero
            // if the point is outside the polygon.
            return (Math.Abs(total_angle) > 0.000001);
        }


        public bool IsInPolygon(decimal longitude, decimal latitude)
        {
           if(
                LineString.Where(lsg => lsg.LineStringType ==ISOLineStringType.PolygonInterior)
                .Any(lsg => IsInPolygonLineString(longitude, latitude, lsg)))
            {
                return false;
            }
            else if (
                LineString.Where(lsg => lsg.LineStringType == ISOLineStringType.PolygonExterior)
                .Any(lsg => IsInPolygonLineString(longitude, latitude, lsg)))
            {
                return true;
            }
            return false;
        }
    }
}
