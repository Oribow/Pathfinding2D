using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NavPosition2d : NavNode2d
{
    public Vector2 Tangent { get { return segment.DirNormalized; } }
    public Vector2 Normal { get { return new Vector2(-segment.DirNormalized.y, segment.DirNormalized.x); } }

    [SerializeField]
    private NavSegment segment;

    public NavPosition2d(Vector2 position, NavSegment segment): base(position, false)
    {
        this.segment = segment;
    }

    public void BuildConnections()
    {
        NavNodeConnection prevConn;
        NavNodeConnection nextConn;
        segment.GetConnectionsFor(this.Position, out prevConn, out nextConn);
        this.Prev = prevConn;
        this.Next = nextConn;
    }

    public void InsertIntoGraph()
    {
        segment.InsertNode(this);
    }

    public void RemoveFromGraph()
    {
        segment.RemoveNode(this);
    }
}
