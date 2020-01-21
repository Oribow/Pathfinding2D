using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NavLine
{
    [SerializeField]
    public NavSegment[] segments;

    public NavLine(List<NavSegment> segments)
    {
        this.segments = segments.ToArray();

        var lastSegment = this.segments[this.segments.Length - 1];
        var lastNode = new NavNode2d(lastSegment.End, false);

        if (this.segments.Length > 1)
        {
            this.segments[0].NavNode.Next = new NavNodeConnection(this.segments[1].NavNode,
                this.segments[0].CompleteTraversalCost);



            for (int iSeg = 1; iSeg < this.segments.Length - 1; iSeg++)
            {
                var prevSeg = this.segments[iSeg - 1];
                var seg = this.segments[iSeg];
                var nextSeg = this.segments[iSeg + 1];

                seg.NavNode.Prev = new NavNodeConnection(
                    prevSeg.NavNode,
                    prevSeg.CompleteTraversalCost);

                seg.NavNode.Next = new NavNodeConnection(
                    nextSeg.NavNode,
                    seg.CompleteTraversalCost);
            }


            var prevLastSegment = this.segments[this.segments.Length - 2];
            lastSegment.NavNode.Prev = new NavNodeConnection(prevLastSegment.NavNode,
                prevLastSegment.CompleteTraversalCost);
        }


        lastSegment.NavNode.Next = new NavNodeConnection(lastNode, lastSegment.CompleteTraversalCost);
        lastNode.Prev = new NavNodeConnection(lastSegment.NavNode, lastSegment.CompleteTraversalCost);
    }

    public float DistanceToPoint(Vector2 point, out NavSegment closestSegment, out Vector2 closestPoint)
    {
        float minDistance = float.MaxValue;
        closestSegment = null;
        closestPoint = Vector2.zero;

        Vector2 a = segments[0].Start;
        NavSegment prevSeg = segments[0];
        for (int iSeg = 1; iSeg < segments.Length; iSeg++)
        {
            Vector2 b = segments[iSeg].Start;
            Vector2 ab = (b - a);
            float l2 = ab.sqrMagnitude;

            Vector2 ap = point - a;

            float dot = Mathf.Clamp01(Vector2.Dot(ab, ap) / l2);
            Vector2 projection = a + dot * ab;
            float dist = Vector2.Distance(projection, point);

            if (dist < minDistance)
            {
                minDistance = dist;
                closestSegment = prevSeg;
                closestPoint = projection;
            }
            prevSeg = segments[iSeg];
            a = b;
        }
        return minDistance;
    }
}

[System.Serializable]
public class NavSegment
{
    public Vector2 Start { get { return NavNode.Position; } }
    public Vector2 End { get { return GetPosition(Length); } }
    public float CompleteTraversalCost { get { return GetCosts(Length); } }
    public Vector2 Normal { get { return new Vector2(-DirNormalized.y, DirNormalized.x); } }
    public Vector2 DirNormalized { get { return dirNorm; } }
    public float Length { get { return length; } }
    public NavNode2d NavNode { get { return node; } }

    [SerializeField]
    private NavNode2d node;
    [SerializeField]
    private float length;
    [SerializeField]
    private Vector2 dirNorm;

    public NavSegment(Vector2 start, Vector2 dirNorm, float length)
    {
        this.dirNorm = dirNorm;
        this.length = length;
        this.node = new NavNode2d(start, false);
    }

    public NavSegment(Vector2 start, Vector2 end)
    {
        var dir = end - start;
        this.length = dir.magnitude;
        this.dirNorm = dir / this.Length;
        this.node = new NavNode2d(start, false);
    }

    public Vector2 GetPosition(float t)
    {
        return Start + dirNorm * t;
    }

    public void GetConnectionsFor(Vector2 point, out NavNodeConnection prevConn, out NavNodeConnection nextConn)
    {
        float distanceSqr = (point - Start).sqrMagnitude;
        Debug.Assert(distanceSqr <= length);
        var n = node;
        do
        {
            n = n.Next.node;
        } while (distanceSqr > (n.Position - Start).sqrMagnitude);

        var prev = n.Prev.node;
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
        Debug.Assert(distanceSqr <= length);
        var n = node;
        do
        {
            n = n.Next.node;
        } while (distanceSqr > (n.Position - Start).sqrMagnitude);

        // prev(l) <--> newlink <--> n
        var prev = n.Prev.node;
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
        var n = node;
        do
        {
            n = n.Next.node;
        } while (n != node);
        var prev = n.Prev.node;
        var next = n.Next.node;

        float distance = Vector2.Distance(prev.Position, next.Position);
        prev.Next = new NavNodeConnection(next, distance);
        next.Prev = new NavNodeConnection(prev, distance);
    }

    public float GetCosts(float dist)
    {
        return dist;
    }
}