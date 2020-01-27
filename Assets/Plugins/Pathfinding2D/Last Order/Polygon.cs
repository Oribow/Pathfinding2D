using ClipperLib;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Polygon : System.IEquatable<Polygon>
{
    public IntRect BoundingRect { get { return boundingRect; } }

    [SerializeField]
    public readonly Contour hull;
    [SerializeField]
    public readonly List<Contour> holes;

    [SerializeField]
    private Polygon parent;
    [SerializeField]
    private Collider2D colliderAdded;
    [SerializeField]
    private IntRect boundingRect;

    public Polygon(Polygon parent, Collider2D colliderAdded, Contour hull)
    {
        this.hull = hull;
        this.holes = new List<Contour>(2);
        this.parent = parent;
        this.colliderAdded = colliderAdded;

        UpdateBounds();
    }

    public void UpdateBounds()
    {
        boundingRect = ABC.Utility.CalculateBoundingRect(hull.Verts);
    }

    public void AddToClipper(Clipper clipper, PolyType polyType)
    {
        clipper.AddPath(hull.Verts, polyType, true);
        foreach (var hole in holes)
        {
            clipper.AddPath(hole.Verts, polyType, true);
        }
    }

    public Polygon GetPolygonBeforeColliderAdded(Collider2D collider)
    {
        Polygon prev = parent;
        while (prev != null && prev.colliderAdded != collider)
        {
            prev = prev.parent;
        }

        if (prev != null)
        {
            return prev.parent;
        }
        return null;
    }

    public bool Equals(Polygon other)
    {
        return other == this;
    }
}
