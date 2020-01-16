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
        if (start == end)
            return;

        var currentNode = freeSegments.First;
        do
        {
            if (currentNode.Value.start >= start && currentNode.Value.end <= end)
            {
                freeSegments.Remove(currentNode);
                continue;
            }
            else if (currentNode.Value.start < start && currentNode.Value.end > end)
            {
                // split
                freeSegments.AddAfter(currentNode, new FreeSegment(end, currentNode.Value.end));
                currentNode.Value = new FreeSegment(currentNode.Value.start, start);
                return;
            }
            else if (currentNode.Value.start <= start && currentNode.Value.end > start)
            {
                currentNode.Value = new FreeSegment(currentNode.Value.start, start);
            }
            else if (currentNode.Value.start > end && currentNode.Value.end <= end)
            {
                currentNode.Value = new FreeSegment(end, currentNode.Value.end);
            }
        } while ((currentNode = currentNode.Next) != null);
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
