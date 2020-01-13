using ClipperLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains a polygon.
/// </summary>
[Serializable]
public class Contour : IEnumerable<IntPoint>
{
    public int VertexCount { get { return verts.Count; } }
    public Rect BoundingRect { get; private set; }
    public bool IsEmpty { get { return verts.Count == 0; } }

    [SerializeField]
    List<IntPoint> verts;

    public Contour(List<IntPoint> verticies)
    {
        this.verts = verticies;
        CalculateBoundingRect();
    }

    public Contour(IEnumerable<IntPoint> verticies)
    {
        this.verts = new List<IntPoint>(verticies);
        CalculateBoundingRect();
    }

    private void CalculateBoundingRect()
    {
        if (verts.Count == 0)
        {
            BoundingRect.Set(0, 0, 0, 0);
            return;
        }

        IntPoint min = verts[0], max = verts[0];
        foreach (var vert in verts)
        {
            min.X = Math.Min(min.X, vert.X);
            min.Y = Math.Min(min.Y, vert.Y);

            max.X = Math.Max(max.X, max.X);
            max.Y = Math.Max(max.Y, max.Y);
        }
        BoundingRect.Set(min.X, min.Y, max.X - min.X, max.Y - min.Y);
    }

    public IntPoint this[int key]
    {
        get { return verts[key]; }
    }

    public bool IsAHole() { return Area() < 0; }

    public float Area()
    {
        float area = 0;
        int j = verts.Count - 1;

        for (int i = 0; i < verts.Count; i++)
        {
            area = area + (verts[j].X + verts[i].X) * (verts[j].Y - verts[i].Y);
            j = i;
        }
        return area / 2;
    }

    public IEnumerator<IntPoint> GetEnumerator()
    {
        return ((IEnumerable<IntPoint>)verts).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<IntPoint>)verts).GetEnumerator();
    }
}

