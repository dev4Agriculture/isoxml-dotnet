using System;

namespace Dev4Agriculture.ISO11783.ISOXML
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
        public static float CrossProductLength(float ax, float ay,
            float bx, float by, float cx, float cy)
        {
            // Get the vectors' coordinates.
            var bAx = ax - bx;
            var bAy = ay - by;
            var bCx = cx - bx;
            var bCy = cy - by;

            // Calculate the Z coordinate of the cross product.
            return bAx * bCy - bAy * bCx;
        }
        // Return the dot product AB . BC.
        // Note that AB x BC = |AB| * |BC| * Cos(theta).
        private static float DotProduct(float ax, float ay,
            float bx, float by, float cx, float cy)
        {
            // Get the vectors' coordinates.
            var bAx = ax - bx;
            var bAy = ay - by;
            var bCx = cx - bx;
            var bCy = cy - by;

            // Calculate the dot product.
            return bAx * bCx + bAy * bCy;
        }
        // Return the angle ABC.
        // Return a value between PI and -PI.
        // Note that the value is the opposite of what you might
        // expect because Y coordinates increase downward.
        public static float GetAngle(float ax, float ay,
            float bx, float by, float cx, float cy)
        {
            // Get the dot product.
            var dot_product = DotProduct(ax, ay, bx, by, cx, cy);

            // Get the cross product.
            var cross_product = CrossProductLength(ax, ay, bx, by, cx, cy);

            // Calculate the angle.
            return (float)Math.Atan2(cross_product, dot_product);
        }
    }
}
