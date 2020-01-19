using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstructableSegment
{
    public Vector2 startPoint;
    public Vector2 dir;
    public LinkedList<FreeSegment> freeSegments;

    public ObstructableSegment(Vector2 startPoint, Vector2 dir)
    {
        this.startPoint = startPoint;
        this.dir = dir;
        freeSegments = new LinkedList<FreeSegment>();
        freeSegments.AddFirst(new FreeSegment(0, 1));
    }

    public void AddObstruction(float start, float end)
    {
        start = Mathf.Clamp01(start);
        end = Mathf.Clamp01(end);

        if (start == end)
            return;

        var currentNode = freeSegments.First;
        while (currentNode != null)
        {
            if (currentNode.Value.start >= start && currentNode.Value.end <= end)
            {
                var next = currentNode.Next;
                freeSegments.Remove(currentNode);
                currentNode = next;
                continue;
            }
            else if (currentNode.Value.start < start && currentNode.Value.end > end)
            {
                // split
                freeSegments.AddAfter(currentNode, new FreeSegment(end, currentNode.Value.end));
                currentNode.Value = new FreeSegment(currentNode.Value.start, start);
                return;
            }
            else if (currentNode.Value.start <= start && currentNode.Value.end > start && currentNode.Value.end <= end )
            {
                currentNode.Value = new FreeSegment(currentNode.Value.start, start);
            }
            else if (currentNode.Value.start < end && currentNode.Value.end >= end && currentNode.Value.start >= start )
            {
                currentNode.Value = new FreeSegment(end, currentNode.Value.end);
            }
            currentNode = currentNode.Next;
        }
    }

    public Vector2 GetPointAlongSegment(float t)
    {
        return startPoint + dir * t;
    }
}

public struct FreeSegment
{
    public float start;
    public float end;

    public FreeSegment(float start, float end)
    {
        this.start = start;
        this.end = end;
    }
}
