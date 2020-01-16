using ClipperLib;
using System.Collections.Generic;
using UnityEngine;

public class Polygon : System.IEquatable<Polygon>
{
    public Contour hull;
    public List<Contour> holes;
    public IntRect BoundingRect { get; private set; }

    public Polygon(Contour hull)
    {
        this.hull = hull;
        this.holes = new List<Contour>(2);
        UpdateBounds();
    }

    public void UpdateBounds()
    {
        BoundingRect = ABC.Utility.CalculateBoundingRect(hull.Verts);
    }

    public void AddToClipper(Clipper clipper, PolyType polyType)
    {
        clipper.AddPath(hull.Verts, polyType, true);
        foreach (var hole in holes)
        {
            clipper.AddPath(hole.Verts, polyType, true);
        }
    }

    public int CompareTo(Polygon other)
    {
        throw new System.NotImplementedException();
    }

    public bool Equals(Polygon other)
    {
        return other == this;
    }
}
