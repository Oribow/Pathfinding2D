using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NavLine
{
    public bool IsClosed { get { return isClosed; } }

    [SerializeField]
    public NavSegment[] segments;

    [System.NonSerialized]
    public int lastNavNodeIndex;

    private bool isClosed;

    public NavLine(List<NavSegment> segments, bool isClosed)
    {
        this.segments = segments.ToArray();
        this.isClosed = isClosed;
        Debug.Assert(isClosed == this.segments.Length >= 3);
    }

    public float DistanceToPoint(Vector2 point, out int closestSegment, out Vector2 closestPoint)
    {
        float minDistance = float.MaxValue;
        closestSegment = -1;
        closestPoint = Vector2.zero;

        Vector2 a, b, ab, ap, projection;
        float l2, dot, dist;

        a = segments[0].Start;
        int prevSeg = 0;
        for (int iSeg = 1; iSeg < segments.Length; iSeg++)
        {
            b = segments[iSeg].Start;
            ab = (b - a);
            l2 = ab.sqrMagnitude;

            ap = point - a;

            dot = Mathf.Clamp01(Vector2.Dot(ab, ap) / l2);
            projection = a + dot * ab;
            dist = Vector2.Distance(projection, point);

            if (dist < minDistance)
            {
                minDistance = dist;
                closestSegment = prevSeg;
                closestPoint = projection;
            }
            prevSeg = iSeg;
            a = b;
        }

        b = segments[prevSeg].End;
        ab = (b - a);
        l2 = ab.sqrMagnitude;

        ap = point - a;

        dot = Mathf.Clamp01(Vector2.Dot(ab, ap) / l2);
        projection = a + dot * ab;
        dist = Vector2.Distance(projection, point);

        if (dist < minDistance)
        {
            minDistance = dist;
            closestSegment = prevSeg;
            closestPoint = projection;
        }

        return minDistance;
    }
}

[System.Serializable]
public class NavSegment
{
    public Vector2 Start { get { return start; } }
    public Vector2 End { get { return GetPosition(Length); } }
    public float TotalTraversalCost { get { return GetCosts(Length); } }
    public Vector2 Normal { get { return new Vector2(-Tangent.y, Tangent.x); } }
    public Vector2 Tangent { get { return dirNorm; } }
    public float Length { get { return length; } }

    [SerializeField]
    Vector2 start;
    [SerializeField]
    private float length;
    [SerializeField]
    private Vector2 dirNorm;

    [System.NonSerialized]
    public int navNodeIndex;

    public NavSegment(Vector2 start, Vector2 dirNorm, float length)
    {
        this.start = start;
        this.dirNorm = dirNorm;
        this.length = length;
    }

    public NavSegment(Vector2 start, Vector2 end)
    {
        this.start = start;
        var dir = end - start;
        this.length = dir.magnitude;
        this.dirNorm = dir / this.Length;
    }

    public Vector2 GetPosition(float t)
    {
        return Start + dirNorm * t;
    }

    public void GetConnectionsFor(Vector2 position, out NavNodeConnection prevConn, out NavNodeConnection nextConn)
    {
        float distanceSqr = (position - Start).sqrMagnitude;
        Debug.Assert(distanceSqr <= length * length);

        int nIndex = navNodeIndex;
        NavNode2d n = NavGraph2d.Instance.GetNode(nIndex);
        do
        {
            nIndex = n.Next.goalNodeIndex;
            n = NavGraph2d.Instance.GetNode(nIndex);
        } while (distanceSqr > (n.Position - Start).sqrMagnitude);

        // prev(l) <--> newlink <--> n
        var prev = NavGraph2d.Instance.GetNode(n.Prev.goalNodeIndex);
        var next = n;

        float costsToNext = GetCosts(Vector2.Distance(next.Position, position));
        float costsToPrev = GetCosts(Vector2.Distance(prev.Position, position));

        prevConn = new NavNodeConnection(n.Prev.goalNodeIndex, costsToPrev);
        nextConn = new NavNodeConnection(nIndex, costsToNext);
    }

    public float GetCosts(float dist)
    {
        return dist;
    }
}