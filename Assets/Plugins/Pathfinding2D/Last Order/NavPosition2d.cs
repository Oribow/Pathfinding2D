using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NavPosition2d
{
    public int LineIndex { get { return lineIndex; } }
    public Vector2 Position { get { return position; } }
    public int SegmentIndex { get { return segmentIndex; } }

    [SerializeField]
    private int segmentIndex;
    [SerializeField]
    private int lineIndex;
    [SerializeField]
    private Vector2 position;

    public NavPosition2d(Vector2 position, int lineIndex, int segmentIndex)
    {
        this.lineIndex = lineIndex;
        this.segmentIndex = segmentIndex;
        this.position = position;
    }
}
