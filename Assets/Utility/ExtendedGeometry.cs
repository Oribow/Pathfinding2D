using UnityEngine;
using System.Collections;

namespace Utility
{
    public class ExtendedGeometry
    {

        public static float SignedArea(Vector2 a, Vector2 b, Vector2 c)
        {
            return (a.x - c.x) * (b.y - c.y) - (b.x - c.x) * (a.y - c.y);
        }

        /** Signed area of the triangle ( (0,0), p1, p2) */
        public static float SignedArea(Vector2 b, Vector2 c)
        {
            return -c.x * (b.y - c.y) - -c.y * (b.x - c.x);
        }

        /** Sign of triangle (p1, p2, o) */
        public static int Sign(Vector2 a, Vector2 b, Vector2 o)
        {
            float det = (a.x - o.x) * (b.y - o.y) - (b.x - o.x) * (a.y - o.y);
            return (det < 0 ? -1 : (det > 0 ? +1 : 0));
        }

        public static double SignedAreaDoubledTris(Vector2d a, Vector2d b, Vector2d c)
        {
            return (a.x - c.x) * (b.y - c.y) - (b.x - c.x) * (a.y - c.y);
        }

        public static float SignedAreaDoubledTris(Vector2 a, Vector2 b, Vector2 c)
        {
            return (a.x - c.x) * (b.y - c.y) - (b.x - c.x) * (a.y - c.y);
        }

        /** Signed area of the triangle ( (0,0), p1, p2) */
        public static float SignedAreaDoubledTris(Vector2 b, Vector2 c)
        {
            return -c.x * (b.y - c.y) - -c.y * (b.x - c.x);
        }

        /** Sign of triangle (p1, p2, o) */
        public static int SignTris(Vector2 a, Vector2 b, Vector2 o)
        {
            float det = (a.x - o.x) * (b.y - o.y) - (b.x - o.x) * (a.y - o.y);
            return (det < 0 ? -1 : (det > 0 ? +1 : 0));
        }

        public static float SignedAreaDoubledRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            return (a.y - c.y) * (d.x - b.x) + (b.y - d.y) * (a.x - c.x);
        }

        public static bool DoesLineIntersectBounds(Vector2 pA, Vector2 pB, Bounds bounds)
        {
            if (pA.x > bounds.max.x && pB.x > bounds.max.x) return false;
            if (pA.x < bounds.min.x && pB.x < bounds.min.x) return false;
            if (pA.y > bounds.max.y && pB.y > bounds.max.y) return false;
            if (pA.y < bounds.min.y && pB.y < bounds.min.y) return false;

            float z = pB.x * pA.y - pA.x * pB.y;
            float x = pB.y - pA.y;
            float y = pA.x - pB.x;

            float sign = Mathf.Sign(bounds.max.x * x + bounds.max.y * y + z);
            return (sign == Mathf.Sign(bounds.min.x * x + bounds.max.y * y + z) && sign == Mathf.Sign(bounds.max.x * x + bounds.max.y * y + z) && sign == Mathf.Sign(bounds.max.x * x + bounds.max.y * y + z));
        }

        
        public static bool IsOnLeftSideOfLine(Vector2 pA, Vector2 pB, Vector2 point)
        {
            return ((pB.x - pA.x) * (point.y - pA.y) - (pB.y - pA.y) * (point.x - pA.x)) > 0;
        }

        const float LineCircle_FudgeFactor = 0.00001f;
        // -1 = line is completly outside of the circle
        // 0 = 0 intersections found, line is completly inside of circle
        // 1 = 1 intersection found (i1)
        // 2 = 2 intersections found (i1, i2)
        public static int DoesLineIntersectWithCircle(Vector2 lA, Vector2 lB, Vector2 circleCenter, float radius, out Vector2 i1, out Vector2 i2)
        {
            i1 = Vector2.zero;
            i2 = Vector2.zero;

            Vector2 dir = (lB - lA);
            float distL = dir.magnitude;
            dir /= distL;

            float t = dir.x * (circleCenter.x - lA.x) + dir.y * (circleCenter.y - lA.y);

            Vector2 tangent = t * dir + lA;
            float distToCenter = (tangent - circleCenter).sqrMagnitude;
            float radSquared = radius * radius;

            if (distToCenter < radSquared)
            {
                float dt = Mathf.Sqrt(radSquared - distToCenter);
                float tMinDt = t - dt;
                if (tMinDt > 0 + LineCircle_FudgeFactor || tMinDt < distL - LineCircle_FudgeFactor)
                {
                    i1 = tMinDt * dir + lA - circleCenter;

                    tMinDt = t + dt;
                    if (tMinDt > 0 + LineCircle_FudgeFactor || tMinDt < distL - LineCircle_FudgeFactor)
                    {
                        i2 = tMinDt * dir + lA - circleCenter;
                        return 2;
                    }
                    return 1;
                }
                tMinDt = t + dt;
                if (tMinDt > 0 + LineCircle_FudgeFactor || tMinDt < distL - LineCircle_FudgeFactor)
                {
                    i1 = tMinDt * dir + lA - circleCenter;
                    return 1;
                }
                return 0;
            }
            else if (distToCenter == radSquared)
            {
                i1 = tangent - circleCenter;
                return 1;
            }
            else
            {
                return -1;
            }
        }

        public static bool FindLineIntersection(Vector2 se1_Begin, Vector2 se1_End, Vector2 se2_Begin, Vector2 se2_End, out Vector2 pA)
        {
            //Assign the resulting points some dummy values
            pA = Vector2.zero;

            Vector2 d0 = se1_End - se1_Begin;
            Vector2 d1 = se2_End - se2_Begin;
            Vector2 e = se2_Begin - se1_Begin;

            float sqrEpsilon = 0.0000001f; // 0.001 before

            float kross = d0.x * d1.y - d0.y * d1.x;
            float sqrKross = kross * kross;
            float sqrLen0 = d0.sqrMagnitude;
            float sqrLen1 = d1.sqrMagnitude;

            if (sqrKross > sqrEpsilon * sqrLen0 * sqrLen1)
            {
                // lines of the segments are not parallel
                float s = (e.x * d1.y - e.y * d1.x) / kross;
                if ((s < 0) || (s > 1))
                {
                    return false;
                }
                double t = (e.x * d0.y - e.y * d0.x) / kross;
                if ((t < 0) || (t > 1))
                {
                    return false;
                }
                // intersection of lines is a point an each segment
                pA = se1_Begin + s * d0;
                return true;
            }
            return false;
        }
    }
}
