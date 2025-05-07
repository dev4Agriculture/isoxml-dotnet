using System;
using System.Net;

namespace Dev4Agriculture.ISO11783.ISOXML.Utils
{
    public class MathUtils
    {
        // Return the cross product AB x BC.
        // The cross product is a vector perpendicular to AB
        // and BC having length |AB| * |BC| * Sin(theta) and
        // with direction given by the right-hand rule.
        // For two vectors in the X-Y plane, the result is a
        // vector with X and Y components 0 so the Z component
        // gives the vector's length and direction.
        public static float CrossProductLength(float Ax, float Ay,
            float Bx, float By, float Cx, float Cy)
        {
            // Get the vectors' coordinates.
            var BAx = Ax - Bx;
            var BAy = Ay - By;
            var BCx = Cx - Bx;
            var BCy = Cy - By;

            // Calculate the Z coordinate of the cross product.
            return BAx * BCy - BAy * BCx;
        }
        // Return the dot product AB . BC.
        // Note that AB x BC = |AB| * |BC| * Cos(theta).
        private static float DotProduct(float Ax, float Ay,
            float Bx, float By, float Cx, float Cy)
        {
            // Get the vectors' coordinates.
            var BAx = Ax - Bx;
            var BAy = Ay - By;
            var BCx = Cx - Bx;
            var BCy = Cy - By;

            // Calculate the dot product.
            return BAx * BCx + BAy * BCy;
        }
        // Return the angle ABC.
        // Return a value between PI and -PI.
        // Note that the value is the opposite of what you might
        // expect because Y coordinates increase downward.
        public static float GetAngle(float Ax, float Ay,
            float Bx, float By, float Cx, float Cy)
        {
            // Get the dot product.
            var dot_product = DotProduct(Ax, Ay, Bx, By, Cx, Cy);

            // Get the cross product.
            var cross_product = CrossProductLength(Ax, Ay, Bx, By, Cx, Cy);

            // Calculate the angle.
            return (float)Math.Atan2(cross_product, dot_product);
        }


        /// <summary>
        /// This function is used to calculate a cleaned Average for a single range of average values. It expects that at start, already a countAtStart amount of values have been
        /// used to build the start average. We calculate the Average that was really generated over all values within the range of start to end, nothing before.
        /// </summary>
        /// <param name="startAverage">Average Value at start of measurement</param>
        /// <param name="countAtStart">Number of values used to build this average</param>
        /// <param name="endAverage">Average value at end of measurement</param>
        /// <param name="countAtEnd">Total Number of values used for measurement; from 0 to start to end</param>
        /// <returns></returns>
        public static double CalculateCleanedContinousWeightedAverage(double startAverage, long countAtStart, double endAverage, long countAtEnd)
        {
            var count = countAtEnd - countAtStart;
            if (count == 0)
            {
                return endAverage;
            }
            var segmentSum = endAverage * countAtEnd - startAverage * countAtStart;
            return segmentSum / count;


        }
    }
}
