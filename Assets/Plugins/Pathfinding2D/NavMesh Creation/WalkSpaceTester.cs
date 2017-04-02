//#define DEBUG_SELF_EDGE
//#define DEBUG_SELF_CIRCLE
//#define DEBUG_OTHER_EDGE
//#define DEBUG_OTHER_CIRCLE
//#define DEBUG_CIRCLE_SECTOR_INTERSECTION_TEST


using UnityEngine;
using Utility;
using System.Collections.Generic;
using Utility.ExtensionMethods;

namespace NavGraph.Build
{
    internal class WalkSpaceTester
    {
        private const float fudgeFactor = 0.0001f;
        private const float maxVertDeviation = 0.01f;

        public static void MarkNotWalkableSegments(MarkableContour sp, List<ExpandedNode> cps, float minWalkableHeight)
        {
            Bounds inflatedBounds;
            int segmentCount;
            Segment firstExSeg = InflateContour(sp, minWalkableHeight, out inflatedBounds, out segmentCount);
            Segment pSeg = firstExSeg;


            MarkSelfIntersections(firstExSeg, sp, minWalkableHeight);
            foreach (ExpandedNode cp in cps)
            {
                if (cp.contour == sp)
                    continue;
                if (cp.contour.bounds.Intersects(sp.bounds))
                    MarkIntersections(firstExSeg, cp.contour, minWalkableHeight);
            }
        }

        public static void MarkSelfIntersections(MarkableContour sp, float minWalkableHeight)
        {
            Bounds inflatedBounds;
            int segmentCount;
            Segment firstExSeg = InflateContour(sp, minWalkableHeight, out inflatedBounds, out segmentCount);
            Segment pSeg = firstExSeg;
            MarkSelfIntersections(firstExSeg, sp, minWalkableHeight);
        }

        private static Segment InflateContour(MarkableContour mC, float minWalkableHeight, out Bounds inflatedBounds, out int segmentCount)
        {
            Segment firstExSeg = null, cExSeg, prevSeg;
            Vector2 normal, tangent;
            segmentCount = 0;

            //Calc lastC at the start. This operation will be done later too though, but its done now for code ease.
            tangent = (mC.firstPoint.pointC - mC.firstPoint.pointB).normalized;
            normal = new Vector2(tangent.y, -tangent.x) * minWalkableHeight;
            prevSeg = new Segment();
            prevSeg.b = mC.firstPoint.pointC + normal;
            firstExSeg = prevSeg;
            inflatedBounds = new Bounds(prevSeg.b, Vector2.zero);

            foreach (PointNode pn in mC)
            {
                tangent = (pn.pointC - pn.pointB).normalized;
                normal = new Vector2(tangent.y, -tangent.x) * minWalkableHeight;
                cExSeg = new Segment();
                cExSeg.src = pn;

                if (pn.angle >= Mathf.PI)
                {
                    cExSeg.a = pn.pointB + normal;
                    cExSeg.b = pn.pointC + normal;

                    Vector2[] aproxVerts = AproximateCircle(cExSeg.a, prevSeg.b, pn.pointB, minWalkableHeight, Mathf.PI * 2 - pn.angle);
                    for (int iAVert = 0; iAVert < aproxVerts.Length - 1; iAVert++)
                    {
                        Segment circleExSeg = new Segment();
                        circleExSeg.a = aproxVerts[iAVert];
                        circleExSeg.b = aproxVerts[iAVert + 1];
                        circleExSeg.isCorner = true;
                        circleExSeg.src = pn;
                        prevSeg.next = circleExSeg;
                        prevSeg = circleExSeg;
                        inflatedBounds.Encapsulate(aproxVerts[iAVert]);
                    }
                    segmentCount += aproxVerts.Length - 1;
                }
                else
                {
                    cExSeg.a = pn.pointB + normal;
                    cExSeg.b = pn.pointC + normal;
                }
                segmentCount++;
                inflatedBounds.Encapsulate(cExSeg.b);
                prevSeg.next = cExSeg;
                prevSeg = cExSeg;
            }
            return firstExSeg.next;
        }

        private static Vector2[] AproximateCircle(Vector2 a, Vector2 b, Vector2 m, float radius, float angle)
        {
            Vector2 nA = a - m;
            Vector2 nB = b - m;

            int S = Mathf.Max(Mathf.CeilToInt(angle / (Mathf.Acos(radius / (radius + maxVertDeviation)) * 2)), 1);
            float correctedL = (radius / Mathf.Cos(angle / (S * 2))) - radius;

            Vector2[] result = new Vector2[S + 2];

            float angleStep = angle / S;

            Vector2 cPoint = nA.RotateRad(angleStep / 2).normalized * (radius + correctedL);
            result[0] = a;
            result[1] = cPoint + m;
            result[S + 1] = b;
            for (int i = 2; i < S + 1; i++)
            {
                cPoint = cPoint.RotateRad(angleStep);
                result[i] = cPoint + m;
            }
            return result;
        }

        private static void MarkSelfIntersections(Segment firstExSeg, MarkableContour sp, float minWalkableHeight)
        {
            Vector2 rectA, rectB, rectC, rectD;
            float doubledAreaOfRect, start = 0, end = 0, dot;
            bool prevWasInside;
            int isInOut;
            Vector2 intersection;
            PointNode pn = null;

            //Circle vars
            Bounds circleBounds;
            float radSquared = minWalkableHeight * minWalkableHeight;
            do
            {
                if (firstExSeg.isCorner)//circle
                {
                    rectA = firstExSeg.a;

                    while (firstExSeg.next.isCorner)
                    {
                        firstExSeg = firstExSeg.next;
                    }

                    rectC = firstExSeg.src.pointB;
                    rectB = firstExSeg.b - rectC;
                    rectA -= rectC;

                    circleBounds = new Bounds(rectC, new Vector3(minWalkableHeight * 2, minWalkableHeight * 2));
#if DEBUG_SELF_CIRCLE
                    DebugExtension.DebugBounds(circleBounds, Color.yellow);
                    DebugExtension.DebugArrow(rectC, rectB, Color.green);
                    DebugExtension.DebugArrow(rectC, rectA, Color.blue);
#endif
                    pn = firstExSeg.next.src.Next;
                    while (true)
                    {
                        if (pn.pointC == rectC) // end reached
                        {
                            break;
                        }
                        if (circleBounds.Contains(pn.pointB))
                        {
                            if (IsPointInsideCircleSector(rectB, rectA, radSquared, pn.pointB - rectC))
                            {
                                //Intersection found!
#if DEBUG_SELF_CIRCLE
                                DebugExtension.DebugArrow(rectC, rectA, Color.gray);
                                DebugExtension.DebugArrow(rectC, rectB, Color.gray);
                                DebugExtension.DebugPoint(pn.pointB, Color.cyan, 2);
#endif
                                firstExSeg.next.src.MarkPointNotWalkable();
                                break;
                            }
                        }

                        if (ExtendedGeometry.DoesLineIntersectBounds(pn.pointB, pn.pointC, circleBounds))
                        {
                            if (DoesLineIntersectCircleSector(pn.pointB, pn.pointC, radSquared, rectC, rectB, rectA))
                            {
#if DEBUG_SELF_CIRCLE
                                DebugExtension.DebugArrow(rectC, pn.pointB - rectC, Color.gray);
                                DebugExtension.DebugArrow(rectC, pn.pointC - rectC, Color.gray);
#endif
                                firstExSeg.next.src.MarkPointNotWalkable();
                                break;
                            }
                        }
                        pn = pn.Next;
                    }
                }
                else
                {
                    rectA = firstExSeg.a;
                    rectB = firstExSeg.b;
                    rectC = firstExSeg.src.pointC;
                    rectD = firstExSeg.src.pointB;
                    doubledAreaOfRect = Mathf.Abs(ExtendedGeometry.SignedAreaDoubledRect(rectA, rectB, rectC, rectD)) + fudgeFactor;
#if DEBUG_SELF_EDGE
                    Debug.DrawLine(rectA, rectB, Color.cyan);
                    Debug.DrawLine(rectB, rectC, Color.cyan);
                    Debug.DrawLine(rectA, rectD, Color.cyan);
#endif

                    prevWasInside = true;
                    end = firstExSeg.src.distanceBC;
                    start = firstExSeg.src.distanceBC;
                    pn = firstExSeg.src.Next.Next;

                    while (true)
                    {
                        if (pn == firstExSeg.src)
                        {
                            if (!prevWasInside)
                            {
                                if (HandlePossibleIntersection(rectA, rectB, pn.Previous, out intersection) || HandlePossibleIntersection(rectB, rectC, pn.Previous, out intersection) || HandlePossibleIntersection(rectA, rectD, pn.Previous, out intersection))
                                {
#if DEBUG_SELF_EDGE
                                    DebugExtension.DebugPoint(intersection);
#endif
                                    dot = Vector2.Dot(firstExSeg.src.tangentBC, intersection - rectA);
                                    end = dot;
                                }
                                else
                                    break;
                            }
                            start = 0;
                            firstExSeg.src.AddObstruction(start, end);
                            break;
                        }
                        if (IsPointInsideRect(doubledAreaOfRect, rectA, rectB, rectC, rectD, pn.pointB))
                        {
                            if (!prevWasInside)
                            {
                                if (HandlePossibleIntersection(rectA, rectB, pn.Previous, out intersection) || HandlePossibleIntersection(rectB, rectC, pn.Previous, out intersection) || HandlePossibleIntersection(rectA, rectD, pn.Previous, out intersection))
                                {
#if DEBUG_SELF_EDGE
                                    DebugExtension.DebugPoint(intersection, Color.yellow);
#endif
                                    dot = Vector2.Dot(firstExSeg.src.tangentBC, intersection - rectA);
                                    start = dot;
                                    end = dot;
                                }

                            }
#if DEBUG_SELF_EDGE
                            DebugExtension.DebugPoint(pn.pointB, Color.black);
#endif
                            dot = Vector2.Dot(firstExSeg.src.tangentBC, pn.pointB - rectA);
                            start = Mathf.Min(dot, start);
                            end = Mathf.Max(dot, end);
                            prevWasInside = true;
                        }
                        else
                        {
                            isInOut = prevWasInside ? 1 : 0;
                            if (HandlePossibleIntersection(rectA, rectB, pn.Previous, out intersection))
                            {
#if DEBUG_SELF_EDGE
                                DebugExtension.DebugPoint(intersection, Color.red);
#endif
                                dot = Vector2.Dot(firstExSeg.src.tangentBC, intersection - rectA);
                                if (isInOut == 0)
                                {
                                    start = dot;
                                    end = dot;
                                }
                                else
                                {
                                    start = Mathf.Min(dot, start);
                                    end = Mathf.Max(dot, end);
                                }
                                isInOut++;
                            }
                            if (HandlePossibleIntersection(rectB, rectC, pn.Previous, out intersection))
                            {
#if DEBUG_SELF_EDGE
                                DebugExtension.DebugPoint(intersection, Color.red);
#endif
                                dot = firstExSeg.src.distanceBC;
                                if (isInOut == 0)
                                {
                                    start = dot;
                                    end = dot;
                                }
                                else
                                {
                                    start = Mathf.Min(dot, start);
                                    end = Mathf.Max(dot, end);
                                }
                                isInOut++;
                            }
                            if (HandlePossibleIntersection(rectA, rectD, pn.Previous, out intersection))
                            {
#if DEBUG_SELF_EDGE
                                DebugExtension.DebugPoint(intersection, Color.red);
#endif
                                dot = 0;
                                if (isInOut == 0)
                                {
                                    start = dot;
                                    end = dot;
                                }
                                else
                                {
                                    start = Mathf.Min(dot, start);
                                    end = Mathf.Max(dot, end);
                                }
                                isInOut++;
                            }
                            if (prevWasInside)
                            {
                                if (isInOut >= 2)
                                    firstExSeg.src.AddObstruction(start, end);
                                else
                                {
                                    start = 0;
                                    end = 0;
                                }
                                prevWasInside = false;
                            }
                            else
                            {
                                if (isInOut == 1)
                                    prevWasInside = true;
                                else if (isInOut == 2)
                                    firstExSeg.src.AddObstruction(start, end);
                                else
                                {
                                    start = 0;
                                    end = 0;
                                }
                            }
                        }
                        pn = pn.Next;
                    }
                }
            } while ((firstExSeg = firstExSeg.next) != null);
        }

        private static void MarkIntersections(Segment firstExSeg, MarkableContour cp, float minWalkableHeight)
        {
            Vector2 rectA, rectB, rectC, rectD;
            float doubledAreaOfRect, start = 0, end = 0, dot;
            bool prevWasInside;
            int isInOut;
            Vector2 intersection;

            //Circle vars
            Bounds circleBounds;
            float radSquared = minWalkableHeight * minWalkableHeight;
#if DEBUG_OTHER_EDGE
            DebugExtension.DebugCircle(cp.firstPoint.pointB, Vector3.forward * 2, Color.magenta);
#endif
            do
            {
                if (firstExSeg.isCorner)//circle
                {
                    if (!firstExSeg.src.IsPointWalkable())
                        continue;

                    rectA = firstExSeg.a;

                    while (firstExSeg.next.isCorner)
                    {
                        firstExSeg = firstExSeg.next;
                    }

                    rectC = firstExSeg.src.pointB;
                    rectB = firstExSeg.b - rectC;
                    rectA -= rectC;

                    circleBounds = new Bounds(rectC, new Vector3(minWalkableHeight * 2, minWalkableHeight * 2));
#if DEBUG_OTHER_CIRCLE
                    DebugExtension.DebugBounds(circleBounds, Color.red);
#endif
                    foreach (PointNode pn in cp)
                    {
                        if (circleBounds.Contains(pn.pointB))
                        {
                            if (IsPointInsideCircleSector(rectB, rectA, radSquared, pn.pointB - rectC))
                            {
                                //Intersection found!
#if DEBUG_OTHER_CIRCLE
                                DebugExtension.DebugBounds(circleBounds, Color.yellow);
                                DebugExtension.DebugArrow(rectC, rectA, Color.gray);
                                DebugExtension.DebugArrow(rectC, rectB, Color.gray);
                                DebugExtension.DebugPoint(pn.pointB, Color.cyan, 2);
#endif
                                firstExSeg.next.src.MarkPointNotWalkable();
                                break;
                            }
                        }
                        if (ExtendedGeometry.DoesLineIntersectBounds(pn.pointB, pn.pointC, circleBounds))
                        {
                            if (DoesLineIntersectCircleSector(pn.pointB, pn.pointC, radSquared, rectC, rectB, rectA))
                            {
                                firstExSeg.next.src.MarkPointNotWalkable();
                                break;
                            }
                        }
                    }
                }
                else
                {
                    rectA = firstExSeg.a;
                    rectB = firstExSeg.b;
                    rectC = firstExSeg.src.pointC;
                    rectD = firstExSeg.src.pointB;
                    doubledAreaOfRect = Mathf.Abs(ExtendedGeometry.SignedAreaDoubledRect(rectA, rectB, rectC, rectD)) + fudgeFactor;
#if DEBUG_OTHER_EDGE
                    Debug.DrawLine(rectA, rectB, Color.cyan);
                    Debug.DrawLine(rectB, rectC, Color.cyan);
                    Debug.DrawLine(rectA, rectD, Color.cyan);
#endif
                    start = 5;
                    end = 50;
                    prevWasInside = false;//IsPointInsideRect(doubledAreaOfRect, rectA, rectB, rectC, rectD, cp.firstPoint.Previous.pointB);


                    foreach (PointNode pn in cp)
                    {
                        if (IsPointInsideRect(doubledAreaOfRect, rectA, rectB, rectC, rectD, pn.pointB))
                        {
                            if (!prevWasInside)
                            {
                                if (HandlePossibleIntersection(rectA, rectB, pn.Previous, out intersection) || HandlePossibleIntersection(rectB, rectC, pn.Previous, out intersection) || HandlePossibleIntersection(rectA, rectD, pn.Previous, out intersection))
                                {
#if DEBUG_OTHER_EDGE
                                    DebugExtension.DebugPoint(intersection, Color.yellow);
                                    DebugExtension.DebugPoint(pn.pointB, Color.black);
#endif
                                    dot = Vector2.Dot(firstExSeg.src.tangentBC, intersection - rectA);
                                    start = dot;
                                    end = dot;
                                    dot = Vector2.Dot(firstExSeg.src.tangentBC, pn.pointB - rectA);
                                    start = Mathf.Min(dot, start);
                                    end = Mathf.Max(dot, end);
                                }
                                else
                                {
#if DEBUG_OTHER_EDGE
                                    DebugExtension.DebugPoint(pn.pointB, Color.black);
#endif
                                    dot = Vector2.Dot(firstExSeg.src.tangentBC, pn.pointB - rectA);
                                    start = dot;
                                    end = dot;
                                }
                            }
                            else
                            {
#if DEBUG_OTHER_EDGE
                                DebugExtension.DebugPoint(pn.pointB, Color.black);
#endif
                                dot = Vector2.Dot(firstExSeg.src.tangentBC, pn.pointB - rectA);
                                start = Mathf.Min(dot, start);
                                end = Mathf.Max(dot, end);
                            }
                            prevWasInside = true;
                        }
                        else
                        {
                            isInOut = prevWasInside ? 1 : 0;
                            if (HandlePossibleIntersection(rectA, rectB, pn.Previous, out intersection))
                            {
#if DEBUG_OTHER_EDGE
                                DebugExtension.DebugPoint(intersection, Color.red);
#endif
                                dot = Vector2.Dot(firstExSeg.src.tangentBC, intersection - rectA);
                                if (isInOut == 0)
                                {
                                    start = dot;
                                    end = dot;
                                }
                                else
                                {
                                    start = Mathf.Min(dot, start);
                                    end = Mathf.Max(dot, end);
                                }
                                isInOut++;
                            }
                            if (HandlePossibleIntersection(rectB, rectC, pn.Previous, out intersection))
                            {
#if DEBUG_OTHER_EDGE
                                DebugExtension.DebugPoint(intersection, Color.red);
#endif
                                dot = firstExSeg.src.distanceBC;
                                if (isInOut == 0)
                                {
                                    start = dot;
                                    end = dot;
                                }
                                else
                                {
                                    start = Mathf.Min(dot, start);
                                    end = Mathf.Max(dot, end);
                                }
                                isInOut++;
                            }
                            if (HandlePossibleIntersection(rectA, rectD, pn.Previous, out intersection))
                            {
#if DEBUG_OTHER_EDGE
                                DebugExtension.DebugPoint(intersection, Color.red);
#endif
                                dot = 0;
                                if (isInOut == 0)
                                {
                                    start = dot;
                                    end = dot;
                                }
                                else
                                {
                                    start = Mathf.Min(dot, start);
                                    end = Mathf.Max(dot, end);
                                }
                                isInOut++;
                            }
                            if (prevWasInside)
                            {
                                if (isInOut >= 2)
                                    firstExSeg.src.AddObstruction(start, end);
                                prevWasInside = false;
                            }
                            else
                            {
                                if (isInOut == 1)
                                    prevWasInside = true;
                                else if (isInOut == 2)
                                    firstExSeg.src.AddObstruction(start, end);
                            }
                        }
                    }
                    if (prevWasInside)
                        firstExSeg.src.AddObstruction(start, end);
                }
            } while ((firstExSeg = firstExSeg.next) != null);
        }

        private static bool HandlePossibleIntersection(Vector2 a, Vector2 b, PointNode pn, out Vector2 intersection)
        {
            intersection = Vector2.zero;
            Vector2 ip1;  // intersection points

            if (!ExtendedGeometry.FindLineIntersection(a, b, pn.pointB, pn.pointC, out ip1))
                return false;

            if (a == ip1 || b == ip1)
                return false;

            intersection = ip1;
            return true;
        }

        private static bool IsPointInsideRect(float doubledAreaOfRect, Vector2 rectA, Vector2 rectB, Vector2 rectC, Vector2 rectD, Vector2 point)
        {

            float totalTriArea = Mathf.Abs(ExtendedGeometry.SignedAreaDoubledTris(rectA, rectB, point));
            if (totalTriArea > doubledAreaOfRect)
                return false;

            totalTriArea += Mathf.Abs(ExtendedGeometry.SignedAreaDoubledTris(rectB, rectC, point));
            if (totalTriArea > doubledAreaOfRect)
                return false;

            totalTriArea += Mathf.Abs(ExtendedGeometry.SignedAreaDoubledTris(rectC, rectD, point));
            if (totalTriArea > doubledAreaOfRect)
                return false;

            totalTriArea += Mathf.Abs(ExtendedGeometry.SignedAreaDoubledTris(rectD, rectA, point));
            if (totalTriArea > doubledAreaOfRect)
                return false;

            return true;
        }

        private static bool IsPointInsideCircleSector(Vector2 a, Vector2 b, float radSquared, Vector2 point)
        {
            //is within radius:
            return ((-a.x * point.y + a.y * point.x > 0 && -b.x * point.y + b.y * point.x <= 0) && point.x * point.x + point.y * point.y <= radSquared);
        }

        private static bool DoesLineIntersectCircleSector(Vector2 lA, Vector2 lB, float radSquared, Vector2 circleCenter, Vector2 sectorStart, Vector2 sectorEnd)
        {
#if DEBUG_CIRCLE_SECTOR_INTERSECTION_TEST
            DebugExtension.DebugArrow(lA, lB - lA, Color.yellow);
#endif
            Vector2 dir = (lB - lA);
            float distL = dir.magnitude;
            dir /= distL;


            float t = dir.x * (circleCenter.x - lA.x) + dir.y * (circleCenter.y - lA.y);

            Vector2 tangent = t * dir + lA;
            float distToCenter = (tangent - circleCenter).sqrMagnitude;


            if (distToCenter < radSquared)
            {
#if DEBUG_CIRCLE_SECTOR_INTERSECTION_TEST
                DebugExtension.DebugCircle(circleCenter, Vector3.forward * 2, Color.red);
                DebugExtension.DebugArrow(sectorStart, sectorEnd - sectorStart, Color.red);
#endif
                float dt = Mathf.Sqrt(radSquared - distToCenter);
                float tMinDt = t - dt;
                if (tMinDt > 0 + fudgeFactor || tMinDt < distL - fudgeFactor)
                {
                    Vector2 i1 = tMinDt * dir + lA - circleCenter;
#if DEBUG_CIRCLE_SECTOR_INTERSECTION_TEST
                    DebugExtension.DebugPoint(i1, Color.clear, 3);
#endif
                    if (-sectorStart.x * i1.y + sectorStart.y * i1.x > 0 && -sectorEnd.x * i1.y + sectorEnd.y * i1.x < 0)
                        return true;
                }
                tMinDt = t + dt;
                if (tMinDt > 0 + fudgeFactor || tMinDt < distL - fudgeFactor)
                {
                    if (t <= 0 + fudgeFactor || t >= distL - fudgeFactor)
                        return false;
                    Vector2 i2 = tMinDt * dir + lA - circleCenter;
#if DEBUG_CIRCLE_SECTOR_INTERSECTION_TEST
                    DebugExtension.DebugPoint(i2, Color.clear, 3);
#endif
                    if ((-sectorStart.x * i2.y + sectorStart.y * i2.x > 0 && -sectorEnd.x * i2.y + sectorEnd.y * i2.x < 0))
                        return true;
                }
                return false;
            }
            else if (distToCenter == radSquared)
            {
                tangent -= circleCenter;
#if DEBUG_CIRCLE_SECTOR_INTERSECTION_TEST
                DebugExtension.DebugCircle(circleCenter, Vector3.forward * 2, Color.red);
                DebugExtension.DebugPoint(tangent, Color.clear, 3);
#endif
                return (-sectorStart.x * tangent.y + sectorStart.y * tangent.x > 0 && -sectorEnd.x * tangent.y + sectorEnd.y * tangent.x < 0);
            }
            else
            {
                return false;
            }
        }

        class Segment
        {
            public float DistanceAB { get { return src.distanceBC; } }
            public Vector2 a;
            public Vector2 b;
            public bool isCorner;
            public Segment next;
            public PointNode src;
        }
    }
}
