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
    PolygonSet polygonSet;
    cInt agentHeight;
    Clipper clipper;


    public NavLine[] Mark(PolygonSet polySet, NavAgentType navAgentType)
    {
        this.agentHeight = polygonSet.FloatToCInt(navAgentType.height);
        this.clipper = new Clipper();
    }

    private NavLine[] MarkContour(int polygonIndex, Contour contour)
    {

        //1. go through every segement
        var verts = contour.Verts;
        var prevPoint = verts[verts.Count - 1];
        ObstructableSegment[] obstructableContour = new ObstructableSegment[verts.Count];

        for (int iPoint = 0; iPoint < verts.Count; iPoint++)
        {
            var point = verts[iPoint];
            Vector2 fPoint = polygonSet.IntPointToVector2(point);
            IntPoint dir = point - prevPoint;
            Vector2 fDir = new Vector2(dir.x, dir.y);
            var seg = new ObstructableSegment(fPoint, fDir);
            fDir.Normalize();

            // 1. filter based on slope angle, everything above --o-- will be discarded
            IntPoint normal = new IntPoint(-dir.y, dir.x);

            if (normal.y <= 0)
            {
                obstructableContour[iPoint] = seg;
                obstructableContour[iPoint].AddObstruction(0, 1);
                continue; // ignore this line, it can't be walked on by a gravity bound character
            }

            // 2. construct parallelogram
            var walkSpace = ConstructWalkSpaceParallelogram(prevPoint, point);
            var walkSpaceBounds = ABC.Utility.CalculateBoundingRect(walkSpace);

            
            List<Vector2> bounds = new List<Vector2>(5);

            // 3. intersect with all other polygons
            for (int iPoly = 0; iPoly < polygonSet.Polygons.Count; iPoly++)
            {
                // skip our self and bounds check
                if (iPoly == polygonIndex || !walkSpaceBounds.Overlaps(polygonSet.Polygons[iPoly].BoundingRect))
                    continue;

                clipper.Clear();
                clipper.AddPath(walkSpace, PolyType.ptClip, true);
                polygonSet.Polygons[iPoly].AddToClipper(clipper, PolyType.ptSubject);

                List<List<IntPoint>> solution = new List<List<IntPoint>>();
                clipper.Execute(ClipType.ctIntersection, solution);

                if (solution.Count == 0)
                    continue; //no intersection

                // every intersection results in a break in the nav line
                // find their respective minima and maxima along the normal
                for (int iSolutionPoly = 0; iSolutionPoly < solution.Count; iSolutionPoly++)
                {
                    bounds.Add(ProjectPolygonOnSegment(solution[iSolutionPoly], point, fDir));
                }
            }

            // 4. divide nav segment based on bounds
            float segLength = new Vector2(dir.x, dir.y).magnitude;
            for (int iBound = 0; iBound < bounds.Count; iBound++)
            {
                float start = bounds[iBound].x / segLength;
                float end = bounds[iBound].y / segLength;
                seg.AddObstruction(start, end);
            }
            prevPoint = point;
        }

        // we should have now a marked version of the polygon
        // convert it to navlines

        // 1. find first free segment
        int startSeg = 0;
        LinkedListNode<FreeSegment> startSegNode;

        for (int iSeg = 0; iSeg < obstructableContour.Length; iSeg++)
        {
            var seg = obstructableContour[iSeg];
            var currentSegNode = seg.freeSegments.First;
            if (currentSegNode != null)
            {
                //get next segment till they arent connected
                var nextSegNode = currentSegNode;
                int nextSeg = iSeg;

                int lastSeg = nextSeg;
                var lastSegNode = nextSegNode;

                do
                {
                    GetNextSegment(obstructableContour, ref nextSeg, ref nextSegNode);

                    if (lastSegNode.Value.end == 1 && nextSegNode.Value.start == 0 && lastSeg + 1 == nextSeg)
                    {

                    }
                    lastSeg = nextSeg;
                    lastSegNode = nextSegNode;

                } while (nextSeg != -1 && nextSegNode != currentSegNode);
            }
            // move left till we find the starting point of this segment
        }

        // 2. go through the rest
        LinkedListNode<FreeSegment> nextSegNode = currentSegNode;
        int nextSeg = iSeg;
        int lastSeg = nextSeg;
        LinkedListNode<FreeSegment> lastSegNode = nextSegNode;

        List<NavLineSegment> navLineSegments = new List<NavLineSegment>(10);
        List<NavLine> navLines = new List<NavLine>(20);
        do
        {
            GetNextSegment(obstructableContour, ref nextSeg, ref nextSegNode);

            if (lastSegNode.Value.end == 1 && nextSegNode.Value.start == 0 && (lastSeg + 1 == nextSeg || lastSeg == nextSeg + obstructableContour.Length))
            {
                // segments connected, add
                navLineSegments.Add(new NavLineSegment(obstructableContour[nextSeg].startPoint));
            }
            else
            {
                // segments not connected, flush

                navLineSegments.Add()
            }
            lastSeg = nextSeg;
            lastSegNode = nextSegNode;

        } while (nextSeg != -1 && nextSegNode != startSegNode);
    }

    private List<IntPoint> ConstructWalkSpaceParallelogram(IntPoint a, IntPoint b)
    {
        List<IntPoint> result = new List<IntPoint>(4);
        result.Add(a);
        result.Add(b);
        a.y += agentHeight;
        b.y += agentHeight;
        result.Add(b);
        result.Add(a);

        return result;
        //MAYBE: need to ensure that the result is ccw
    }

    private Vector2 ProjectPolygonOnSegment(List<IntPoint> polygon, IntPoint segmentStart, Vector2 segmentDir)
    {
        float start, end;
        start = end = DistanceAlongSegement(polygon[0], segmentStart, segmentDir);

        for (int iVert = 1; iVert < polygon.Count; iVert++)
        {
            float dist = DistanceAlongSegement(polygon[iVert], segmentStart, segmentDir);
            start = Mathf.Min(start, dist);
            end = Mathf.Max(end, dist);
        }
        return new Vector2(start, end);
    }

    private float DistanceAlongSegement(IntPoint point, IntPoint segmentStart, Vector2 segmentDir)
    {
        point -= segmentStart;
        return Vector2.Dot(new Vector2(point.x, point.y), segmentDir);
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
}

