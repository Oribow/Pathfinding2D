using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NavPosition2d
{
    public NavLine NavLine { get { return navLine; } }
    public int NavLineSegmentIndex { get { return navLineSegmentIndex; } }
    public Vector2 Point { get { return point; } }

    public Vector2 SegmentTangent { get {
            Vector2 a = navLine.segments[navLineSegmentIndex].start;
            Vector2 b = navLine.segments[navLineSegmentIndex + 1].start;
            return b - a;
        }
    }
    public Vector2 SegmentNormal
    {
        get
        {
            Vector2 t = SegmentTangent;
            return new Vector2(-t.y, t.x);
        }
    }

    [SerializeField]
    private Vector2 point;
    [SerializeField]
    private NavLine navLine;
    [SerializeField]
    private int navLineSegmentIndex;

    public NavPosition2d(NavLine navLine, int navLineSegmentIndex, Vector2 point)
    {
        this.navLine = navLine;
        this.navLineSegmentIndex = navLineSegmentIndex;
        this.point = point;
    }
}
