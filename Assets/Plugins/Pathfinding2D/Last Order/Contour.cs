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
    public int VertexCount { get { return Verts.Count; } }
    public bool IsEmpty { get { return Verts.Count == 0; } }
    public List<IntPoint> Verts { get; private set; }

    public Contour(List<IntPoint> verticies)
    {
        this.Verts = verticies;
    }

    public Contour(IEnumerable<IntPoint> verticies)
    {
        this.Verts = new List<IntPoint>(verticies);
    }

    public void SetVerticies(List<IntPoint> verticies)
    {
        this.Verts = verticies;
    }

    public IntPoint this[int key]
    {
        get { return Verts[key]; }
    }

    public IEnumerator<IntPoint> GetEnumerator()
    {
        return ((IEnumerable<IntPoint>)Verts).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<IntPoint>)Verts).GetEnumerator();
    }
}
