using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NavLine
{
    [SerializeField]
    public NavLineSegment[] segments;

    public NavLine(List<NavLineSegment> segments)
    {
        this.segments = segments.ToArray();
        Debug.Assert(segments.Count >= 2, "NavLine has to have atleast 2 segments");
    }
}

[System.Serializable]
public class NavLineSegment {
    [SerializeField]
    public Vector2 start;

    public NavLineSegment(Vector2 start)
    {
        this.start = start;
    }
}
 