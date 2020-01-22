using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NavLine
{
    [SerializeField]
    public NavSegment[] segments;
    [SerializeField]
    public bool isClosed;

    public NavLine(List<NavSegment> segments, bool isClosed)
    {
        this.segments = segments.ToArray();
        this.isClosed = isClosed;
        Debug.Assert(isClosed == this.segments.Length >= 3);
    }

    public void BuildNavGraph()
    {
        int startIndex;
        NavSegment prevSeg;

        if (isClosed)
        {
            startIndex = 0;
            prevSeg = this.segments[this.segments.Length - 1];

            prevSeg.Next = new NavNodeConnection(this.segments[0], prevSeg.CompleteTraversalCost);
            prevSeg.Prev = new NavNodeConnection(this.segments[this.segments.Length - 2], prevSeg.CompleteTraversalCost);
        }
        else
        {
            startIndex = 1;
            prevSeg = this.segments[0];

            var lastSegment = this.segments[this.segments.Length - 1];
            var lastNode = new NavNode2d(lastSegment.End, false);
            lastNode.Prev = new NavNodeConnection(lastSegment, lastSegment.CompleteTraversalCost);
            lastSegment.Next = new NavNodeConnection(lastNode, lastSegment.CompleteTraversalCost);

            if (this.segments.Length >= 2)
                this.segments[0].Next = new NavNodeConnection(this.segments[1], this.segments[0].CompleteTraversalCost);
        }

        for (int iSeg = startIndex; iSeg < this.segments.Length - 1; iSeg++)
        {
            var seg = this.segments[iSeg];
            var nextSeg = this.segments[iSeg + 1];

            seg.Prev = new NavNodeConnection(
                prevSeg,
                prevSeg.CompleteTraversalCost);

            seg.Next = new NavNodeConnection(
                nextSeg,
                seg.CompleteTraversalCost);

            prevSeg = seg;
        }
      }

    public float DistanceToPoint(Vector2 point, out NavSegment closestSegment, out Vector2 closestPoint)
    {
        float minDistance = float.MaxValue;
        closestSegment = null;
        closestPoint = Vector2.zero;

        Vector2 a, b, ab, ap, projection;
        float l2, dot, dist;

        a = segments[0].Start;
        NavSegment prevSeg = segments[0];
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
            prevSeg = segments[iSeg];
            a = b;
        }

        b = prevSeg.End;
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
public class NavSegment : NavNode2d
{
    public Vector2 Start { get { return Position; } }
    public Vector2 End { get { return GetPosition(Length); } }
    public float CompleteTraversalCost { get { return GetCosts(Length); } }
    public Vector2 Normal { get { return new Vector2(-DirNormalized.y, DirNormalized.x); } }
    public Vector2 DirNormalized { get { return dirNorm; } }
    public float Length { get { return length; } }

    [SerializeField]
    private float length;
    [SerializeField]
    private Vector2 dirNorm;

    public NavSegment(Vector2 start, Vector2 dirNorm, float length) : base(start, false)
    {
        this.dirNorm = dirNorm;
        this.length = length;
    }

    public NavSegment(Vector2 start, Vector2 end) : base(start, false)
    {
        var dir = end - start;
        this.length = dir.magnitude;
        this.dirNorm = dir / this.Length;
    }

    public Vector2 GetPosition(float t)
    {
        return Start + dirNorm * t;
    }

    public void GetConnectionsFor(Vector2 point, out NavNodeConnection prevConn, out NavNodeConnection nextConn)
    {
        float distanceSqr = (point - Start).sqrMagnitude;
        Debug.Assert(distanceSqr <= length);
        NavNode2d n = this;
        do
        {
            n = n.Next.goalNode;
        } while (distanceSqr > (n.Position - Start).sqrMagnitude);

        var prev = n.Prev.goalNode;
        var next = n;

        float costsToNext = GetCosts(Vector2.Distance(next.Position, point));
        float costsToPrev = GetCosts(Vector2.Distance(prev.Position, point));

        prevConn = new NavNodeConnection(prev, costsToPrev);
        nextConn = new NavNodeConnection(next, costsToNext);
    }

    // WARNING: (newNode.Position - Start).sqrMagnitude NEEDS to be <= length
    public void InsertNode(NavNode2d newNode)
    {
        float distanceSqr = (newNode.Position - Start).sqrMagnitude;
        Debug.Assert(distanceSqr <= length * length);
        NavNode2d n = this;
        do
        {
            n = n.Next.goalNode;
        } while (distanceSqr > (n.Position - Start).sqrMagnitude);

        // prev(l) <--> newlink <--> n
        var prev = n.Prev.goalNode;
        var next = n;

        float costsToNext = GetCosts(Vector2.Distance(next.Position, newNode.Position));
        float costsToPrev = GetCosts(Vector2.Distance(prev.Position, newNode.Position));

        prev.Next = new NavNodeConnection(newNode, costsToPrev);
        next.Prev = new NavNodeConnection(newNode, costsToNext);

        newNode.Prev = new NavNodeConnection(prev, costsToPrev);
        newNode.Next = new NavNodeConnection(next, costsToNext);
    }

    public void RemoveNode(NavNode2d node)
    {
        NavNode2d n = this;
        do
        {
            n = n.Next.goalNode;
        } while (n != node);
        var prev = n.Prev.goalNode;
        var next = n.Next.goalNode;

        float distance = Vector2.Distance(prev.Position, next.Position);
        prev.Next = new NavNodeConnection(next, distance);
        next.Prev = new NavNodeConnection(prev, distance);
    }

    public float GetCosts(float dist)
    {
        return dist;
    }
}