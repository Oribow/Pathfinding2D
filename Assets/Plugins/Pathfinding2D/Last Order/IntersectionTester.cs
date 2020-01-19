using ClipperLib;
using NavGraph.Build;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if use_int32
  using cInt = Int32;
#else
using cInt = System.Int64;
#endif

public class IntersectionTester
{
    cInt agentHeight;
    Clipper clipper;
    PolygonSet polygonSet;
    float halfAgentWidth;


    public List<NavLine> Mark(PolygonSet polySet, NavAgentType navAgentType)
    {
        this.agentHeight = polySet.FloatToCInt(navAgentType.height);
        this.halfAgentWidth = polySet.FloatToCInt(navAgentType.width / 2);

        this.clipper = new Clipper();
        this.polygonSet = polySet;

        List<NavLine> inOutNavLines = new List<NavLine>(50);
        for (int iPoly = 0; iPoly < polySet.Polygons.Count; iPoly++)
        {
            var poly = polySet.Polygons[iPoly];
            MarkContour(inOutNavLines, poly.hull);
            for (int iHole = 0; iHole < poly.holes.Count; iHole++)
            {
                MarkContour(inOutNavLines, poly.holes[iHole]);
            }
        }
        return inOutNavLines;
    }

    private void MarkContour(List<NavLine> inOutNavLines, Contour contour)
    {

        //1. go through every segement
        var verts = contour.Verts;
        var prevPoint = verts[verts.Count - 1];
        ObstructableSegment[] obstructableContour = new ObstructableSegment[verts.Count];

        for (int iPoint = 0; iPoint < verts.Count; iPoint++)
        {
            var point = verts[iPoint];
            Vector2 fPrevPoint = new Vector2(prevPoint.x, prevPoint.y);
            IntPoint dir = point - prevPoint;
            Vector2 fDir = new Vector2(dir.x, dir.y);
            var seg = new ObstructableSegment(fPrevPoint, fDir);
            obstructableContour[iPoint] = seg;
            float segmentLength = fDir.magnitude;
            float dirM = fDir.y / fDir.x;
            fDir /= segmentLength;

            // 1. filter based on slope angle, everything above --o-- will be discarded
            Vector2 normal = new Vector2(-dir.y, dir.x);
            /*GizmosQueue.Instance.Enqueue(5, () =>
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine((fPrevPoint + fDir * 0.5f * segmentLength) / polygonSet.floatToIntMult,
                    (fPrevPoint + fDir * 0.5f * segmentLength) / polygonSet.floatToIntMult +
                    polygonSet.IntPointToVector2(normal).normalized
                    );
            });*/

            if (Vector2.Angle(normal, Vector2.up) > 88)
            {
                // ignore this line, it can't be walked on by a gravity bound character
                seg.AddObstruction(0, 1);
            }
            else
            {
                // 2. construct parallelogram
                var walkSpace = ConstructWalkSpaceParallelogram(prevPoint, point, fDir);
                var walkSpaceBounds = ABC.Utility.CalculateBoundingRect(walkSpace);

                foreach (var solution in ClipAgainstAll(walkSpace, walkSpaceBounds)) {
                    for (int iSolutionPoly = 0; iSolutionPoly < solution.Count; iSolutionPoly++)
                    {
                        var bound = GravityBoundProjectPolygonOnSegment(solution[iSolutionPoly], prevPoint, dirM);

                        float start = bound.x / segmentLength;
                        float end = bound.y / segmentLength;
                        seg.AddObstruction(start, end);

                        int copy = iSolutionPoly;
                        GizmosQueue.Instance.Enqueue(5, () =>
                        {
                            Gizmos.color = Color.red;
                            var sol = solution[copy];
                            var prevP = sol[sol.Count - 1];
                            foreach (var p in sol)
                            {
                                Gizmos.DrawLine(polygonSet.IntPointToVector2(prevP), polygonSet.IntPointToVector2(p));
                                prevP = p;
                            }

                        });
                    }
                }
            }
            prevPoint = point;
        }
        ConvertObstructableSegmentsIntoNavLines(inOutNavLines, obstructableContour);
    }

    private void ConvertObstructableSegmentsIntoNavLines(List<NavLine> inOutNavLines, ObstructableSegment[] obstructableContour)
    {
        // we should have now a marked version of the polygon
        // convert it to navlines
        int nextSeg;
        LinkedListNode<FreeSegment> nextSegNode;
        int lastSeg;
        LinkedListNode<FreeSegment> lastSegNode;

        GetASegment(obstructableContour, out nextSeg, out nextSegNode);
        if (nextSeg == -1)
        {
            // every segment is fully obstructed
            return;
        }

        LinkedListNode<FreeSegment> startSegNode = nextSegNode;

        List<NavLineSegment> navLineSegments = new List<NavLineSegment>(10);
        navLineSegments.Add(new NavLineSegment(obstructableContour[nextSeg].GetPointAlongSegment(nextSegNode.Value.start)));

        int firstLineIndex = inOutNavLines.Count;
        do
        {
            navLineSegments.Add(new NavLineSegment(obstructableContour[nextSeg].GetPointAlongSegment(nextSegNode.Value.end)));
            lastSeg = nextSeg;
            lastSegNode = nextSegNode;
            GetNextSegment(obstructableContour, ref nextSeg, ref nextSegNode);

            if (lastSegNode.Value.end != 1 || nextSegNode.Value.start != 0 || (lastSeg + 1 != nextSeg && lastSeg != nextSeg + obstructableContour.Length - 1))
            {
                // segments not connected, flush
                inOutNavLines.Add(new NavLine(navLineSegments));
                navLineSegments.Clear();
                if (nextSeg != -1 && nextSegNode != startSegNode)
                {
                    // prepare for new line
                    navLineSegments.Add(new NavLineSegment(obstructableContour[nextSeg].GetPointAlongSegment(nextSegNode.Value.start)));
                }
            }
        } while (nextSeg != -1 && nextSegNode != startSegNode);

        // check if last segment is connected to first segment
        if (nextSeg != -1 && (lastSeg + 1 == nextSeg || nextSeg + obstructableContour.Length - 1 == lastSeg) && nextSegNode.Value.start == 0 && lastSegNode.Value.end == 1)
        {
            // merge first and last nav line
            var firstLine = inOutNavLines[firstLineIndex];

            var combinedSegments = new List<NavLineSegment>(navLineSegments.Count + firstLine.segments.Length);
            combinedSegments.AddRange(navLineSegments);
            combinedSegments.AddRange(firstLine.segments);

            inOutNavLines[firstLineIndex] = new NavLine(combinedSegments);
        }
    }

    private IEnumerable<List<List<IntPoint>>> ClipAgainstAll(List<IntPoint> targetPoly, IntRect targetPolyBounds)
    {
        // 3. intersect with all other polygons
        for (int iPoly = 0; iPoly < polygonSet.Polygons.Count; iPoly++)
        {
            // bounds check
            if (!targetPolyBounds.Overlaps(polygonSet.Polygons[iPoly].BoundingRect))
                continue;

            clipper.Clear();
            clipper.AddPath(targetPoly, PolyType.ptClip, true);
            polygonSet.Polygons[iPoly].AddToClipper(clipper, PolyType.ptSubject);

            List<List<IntPoint>> solution = new List<List<IntPoint>>();
            clipper.Execute(ClipType.ctIntersection, solution);

            if (solution.Count == 0)
                continue; //no intersection

            yield return solution;
        }
    }

    private List<IntPoint> ConstructWalkSpaceParallelogram(IntPoint a, IntPoint b, Vector2 dir)
    {
        List<IntPoint> result = new List<IntPoint>(4);
        //var fBias = dir * halfAgentWidth;
        //var bias = new IntPoint(Mathf.RoundToInt(fBias.x), Mathf.RoundToInt(fBias.y));

        //a -= bias;
        //b += bias;

        result.Add(b);
        result.Add(a);
        a.y += agentHeight;
        b.y += agentHeight;
        result.Add(a);
        result.Add(b);


        GizmosQueue.Instance.Enqueue(5, () =>
        {
            DebugExtension.DrawArrow(polygonSet.IntPointToVector2(result[0]), polygonSet.IntPointToVector2(result[1]) - polygonSet.IntPointToVector2(result[0]), Color.blue);
            DebugExtension.DrawArrow(polygonSet.IntPointToVector2(result[1]), polygonSet.IntPointToVector2(result[2]) - polygonSet.IntPointToVector2(result[1]), Color.blue);
            DebugExtension.DrawArrow(polygonSet.IntPointToVector2(result[2]), polygonSet.IntPointToVector2(result[3]) - polygonSet.IntPointToVector2(result[2]), Color.blue);
            DebugExtension.DrawArrow(polygonSet.IntPointToVector2(result[3]), polygonSet.IntPointToVector2(result[0]) - polygonSet.IntPointToVector2(result[3]), Color.blue);
        });

        return result;
        //MAYBE: need to ensure that the result is ccw
    }

    private Vector2 GravityBoundProjectPolygonOnSegment(List<IntPoint> polygon, IntPoint segmentStart, float segmentDirM)
    {
        float start, end;
        start = end = GravityBoundDistanceAlongSegment(segmentStart, segmentDirM, polygon[0]);

        for (int iVert = 1; iVert < polygon.Count; iVert++)
        {
            float dist = GravityBoundDistanceAlongSegment(segmentStart, segmentDirM, polygon[iVert]);
            start = Mathf.Min(start, dist);
            end = Mathf.Max(end, dist);
        }
        return new Vector2(start - halfAgentWidth, end + halfAgentWidth);
    }

    private float GravityBoundDistanceAlongSegment(IntPoint segmentStart, float segmentDirM, IntPoint point)
    {
        var v = new Vector2(point.x - segmentStart.x, 0);
        v.y = segmentDirM * v.x;
        return v.magnitude;
    }

    private void GetNextSegment(ObstructableSegment[] obstructableContour, ref int segIndex, ref LinkedListNode<FreeSegment> currentFreeSegment)
    {
        if (currentFreeSegment.Next != null)
        {
            currentFreeSegment = currentFreeSegment.Next;
            return;
        }
        else
        {
            int oldSegIndex = segIndex;
            do
            {
                segIndex = segIndex + 1;
                if (segIndex >= obstructableContour.Length)
                {
                    segIndex = 0;
                }
                if (obstructableContour[segIndex].freeSegments.Count != 0)
                {
                    currentFreeSegment = obstructableContour[segIndex].freeSegments.First;
                    return;
                }

            } while (oldSegIndex != segIndex);
        }

        if (obstructableContour[segIndex].freeSegments.First != null && obstructableContour[segIndex].freeSegments.First != currentFreeSegment)
        {
            currentFreeSegment = obstructableContour[segIndex].freeSegments.First;
            return;
        }

        segIndex = -1;
        currentFreeSegment = null;
    }

    private void GetASegment(ObstructableSegment[] obstructableContour, out int segIndex, out LinkedListNode<FreeSegment> segment)
    {
        segIndex = 0;
        do
        {
            segment = obstructableContour[segIndex].freeSegments.First;
            if (segment != null)
            {
                return;
            }
            segIndex++;
        } while (segIndex < obstructableContour.Length);

        segIndex = -1;
        segment = null;
    }
}

