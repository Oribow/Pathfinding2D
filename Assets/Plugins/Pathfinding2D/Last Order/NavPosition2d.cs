using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NavPosition2d
{
    public int LineIndex { get { return lineIndex; } }
    public Vector2 Position { get { return position; } }
    public int SegmentIndex { get { return segmentIndex; } }
    public NavSurface2d NavSurface { get { return navSurface; } }

    [SerializeField]
    private int segmentIndex;
    [SerializeField]
    private int lineIndex;
    [SerializeField]
    private Vector2 position;
    [SerializeField]
    private NavSurface2d navSurface;

    public NavPosition2d(NavSurface2d navSurface, Vector2 position, int lineIndex, int segmentIndex)
    {
        this.navSurface = navSurface;
        this.lineIndex = lineIndex;
        this.segmentIndex = segmentIndex;
        this.position = position;
    }
}
