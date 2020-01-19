using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NavLine
{
    [SerializeField]
    public NavLineSegment[] segments;

    public bool isClosed;

    public NavLine(List<NavLineSegment> segments, bool isClosed = false)
    {
        this.segments = segments.ToArray();
        this.isClosed = isClosed;
        Debug.Assert(segments.Count >= 2, "NavLine has to have atleast 2 segments");
    }

    public float DistanceToPoint(Vector2 point, out int closestSegmentIndex, out Vector2 closestPoint)
    {
        float minDistance = float.MaxValue;
        closestSegmentIndex = -1;
        closestPoint = Vector2.zero;

        Vector2 a;
        int startIndex;
        if (isClosed)
        {
            a = segments[segments.Length - 1].start;
            startIndex = 0;
        }
        else
        {
            a = segments[0].start;
            startIndex = 1;
        }
        int prevSeg = startIndex;
        for (int iSeg = startIndex; iSeg < segments.Length; iSeg++)
        {
            Vector2 b = segments[iSeg].start;
            Vector2 ab = (b - a);
            float l2 = ab.sqrMagnitude;

            Vector2 ap = point - a;

            float dot = Mathf.Clamp01(Vector2.Dot(ab, ap) / l2);
            Vector2 projection = a + dot * ab;
            float dist = Vector2.Distance(projection, point);

            if (dist < minDistance)
            {
                minDistance = dist;
                closestSegmentIndex = prevSeg;
                closestPoint = projection;
            }
            prevSeg = iSeg;
            a = b;
        }
        return minDistance;
    }
}

[System.Serializable]
public class NavLineSegment
{
    [SerializeField]
    public Vector2 start;

    public NavLineSegment(Vector2 start)
    {
        this.start = start;
    }
}
