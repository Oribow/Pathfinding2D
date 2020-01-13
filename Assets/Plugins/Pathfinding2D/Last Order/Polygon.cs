using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Polygon
{
    public static Polygon FromBoxCollider2D(BoxCollider2D collider)
    {
        Vector2 halfSize = collider.size / 2;
        List<Vector2> verts = new List<Vector2>(4);

        verts.Add(collider.transform.TransformPoint(halfSize + collider.offset));
        verts.Add(collider.transform.TransformPoint(new Vector2(halfSize.x, -halfSize.y) + collider.offset));
        verts.Add(collider.transform.TransformPoint(-halfSize + collider.offset));
        verts.Add(collider.transform.TransformPoint(new Vector2(-halfSize.x, halfSize.y) + collider.offset));

        return new Polygon(new Contour(verts));
    }

    public static Polygon FromCircleCollider2D(CircleCollider2D collider, int circleVertCount, float anglePerCircleVert)
    {
        List<Vector2> verts = new List<Vector2>(circleVertCount);
        for (int i = 0; i < circleVertCount; i++)
        {
            verts.Add(collider.transform.TransformPoint(new Vector2(collider.radius * Mathf.Sin(anglePerCircleVert * i) + collider.offset.x, collider.radius * Mathf.Cos(anglePerCircleVert * i) + collider.offset.y)));
        }

        return new Polygon(new Contour(verts));
    }

    public static Polygon FromPolygonCollider2D(PolygonCollider2D collider, int pathIndex)
    {
        Matrix4x4 localToWorld = collider.transform.localToWorldMatrix;
        List<Vector2> verts = new List<Vector2>(collider.GetPath(pathIndex));
        for (int iVert = 0; iVert < verts.Count; iVert++)
        {
            verts[iVert] = (localToWorld.MultiplyPoint(verts[iVert] + collider.offset));
        }
        return new Polygon(new Contour(verts));
    }

    public Contour hull;
    public List<Contour> holes;

    public Polygon(Contour hull)
    {
        this.hull = hull;
        this.holes = new List<Contour>(2);
    }
}
