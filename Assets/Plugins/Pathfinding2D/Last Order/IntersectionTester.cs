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


        //1. go through every segement
        var verts = contour.Verts;
        var prevPoint = verts[verts.Count - 1];

        foreach (var point in verts)
        {
            // 1. filter based on slope angle, everything above --o-- will be discarded
            IntPoint dir = point - prevPoint;
            IntPoint normal = new IntPoint(-dir.y, dir.x);

            if (normal.y <= 0)
                continue; // ignore this line, it can't be walked on by a gravity bound character

            // 2. construct parallelogram


            prevPoint = point;
        }
    }

    private NavLine[] MarkContour(int polygonIndex, Contour contour, NavAgentType navAgentType)
    {
        List<NavLine> navLine = new List<NavLine>(contour.VertexCount);
        List<NavLineSegment> currentSegments = new List<NavLineSegment>(5);

        //1. go through every segement
        var verts = contour.Verts;
        var prevPoint = verts[verts.Count - 1];

        foreach (var point in verts)
        {
            // 1. filter based on slope angle, everything above --o-- will be discarded
            IntPoint dir = point - prevPoint;
            IntPoint normal = new IntPoint(-dir.y, dir.x);

            if (normal.y <= 0)
                continue; // ignore this line, it can't be walked on by a gravity bound character

            // 2. construct parallelogram
            var walkSpace = ConstructWalkSpaceParallelogram(prevPoint, point);
            var walkSpaceBounds = ABC.Utility.CalculateBoundingRect(walkSpace);

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

                // every intersection results in a break in the nav line

            }

            prevPoint = point;
        }
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
}
