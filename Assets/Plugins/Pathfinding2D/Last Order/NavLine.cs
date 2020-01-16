using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavLine
{
    NavLineSegment[] segments;

    public NavLine(List<NavLineSegment> segments)
    {
        this.segments = segments.ToArray();
    }
}

public class NavLineSegment {
    Vector2 start;

    public NavLineSegment(Vector2 start)
    {
        this.start = start;
    }
}
