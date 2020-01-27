using ClipperLib;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ColliderSet
{
    public static float floatToIntMult = 1000;
    public static IntPoint Vector2ToIntPoint(Vector2 v)
    {
        return new IntPoint(Mathf.RoundToInt(v.x * floatToIntMult), Mathf.RoundToInt(v.y * floatToIntMult));
    }


    private static List<IntPoint> HullFromBoxCollider2D(BoxCollider2D collider)
    {
        Vector2 halfSize = collider.size / 2;
        List<IntPoint> verts = new List<IntPoint>(4);

        var v = collider.transform.TransformPoint(halfSize + collider.offset);
        verts.Add(Vector2ToIntPoint(v));

        v = collider.transform.TransformPoint(new Vector2(halfSize.x, -halfSize.y) + collider.offset);
        verts.Add(Vector2ToIntPoint(v));

        v = collider.transform.TransformPoint(-halfSize + collider.offset);
        verts.Add(Vector2ToIntPoint(v));

        v = collider.transform.TransformPoint(new Vector2(-halfSize.x, halfSize.y) + collider.offset);
        verts.Add(Vector2ToIntPoint(v));

        return verts;
    }

    private static List<IntPoint> HullFromCircleCollider2D(CircleCollider2D collider, int circleVertCount, float anglePerCircleVert)
    {
        List<IntPoint> verts = new List<IntPoint>(circleVertCount);
        for (int i = 0; i < circleVertCount; i++)
        {
            var v = collider.transform.TransformPoint(new Vector2(collider.radius * Mathf.Sin(anglePerCircleVert * i) + collider.offset.x, collider.radius * Mathf.Cos(anglePerCircleVert * i) + collider.offset.y));
            verts.Add(ColliderSet.Vector2ToIntPoint(v));
        }

        return verts;
    }

    private static List<IntPoint> HullFromPolygonCollider2D(PolygonCollider2D collider, int pathIndex)
    {
        Matrix4x4 localToWorld = collider.transform.localToWorldMatrix;
        var path = collider.GetPath(pathIndex);
        List<IntPoint> verts = new List<IntPoint>(path.Length);
        for (int iVert = 0; iVert < path.Length; iVert++)
        {
            verts.Add(ColliderSet.Vector2ToIntPoint(localToWorld.MultiplyPoint(path[iVert] + collider.offset)));
        }
        return verts;
    }

    float anglePerCircleVert;
    int circleVertCount;
    Dictionary<Collider2D, List<IntPoint>[]> colliderPolygons = new Dictionary<Collider2D, List<IntPoint>[]>();
    List<Polygon> polygons;

    private Clipper clipper;

    public ColliderSet(int circleVertCount)
    {
        if (circleVertCount < 3)
            circleVertCount = 3;
        this.circleVertCount = circleVertCount;
        this.anglePerCircleVert = (Mathf.PI * 2) / circleVertCount;
    }

    public void AddCollider(Collider2D collider)
    {
        Type cTyp = collider.GetType();
        if (cTyp == typeof(EdgeCollider2D))
        {
            //AddEdgeCollider((EdgeCollider2D)col);
            return;
        }

        List<IntPoint>[] polygons = null;
        if (cTyp == typeof(BoxCollider2D))
        {
            polygons = new List<IntPoint>[] { HullFromBoxCollider2D((BoxCollider2D)collider) };
        }
        else if (cTyp == typeof(CircleCollider2D))
        {
            polygons = new List<IntPoint>[]{ HullFromCircleCollider2D((CircleCollider2D)collider, circleVertCount, anglePerCircleVert) };
        }
        else if (cTyp == typeof(PolygonCollider2D))
        {
            PolygonCollider2D pCol = (PolygonCollider2D)collider;
            polygons = new List<IntPoint>[pCol.pathCount];

            for (int iPath = 0; iPath < pCol.pathCount; iPath++)
            {
                polygons[iPath] = HullFromPolygonCollider2D(pCol, iPath);
            }
        }
        Debug.Assert(polygons != null, "Unknown polygon type: " + cTyp);
        colliderPolygons.Add(collider, polygons);
    }

    public void RemoveCollider(Collider2D collider)
    {
        colliderPolygons.Remove(collider);
    }

    public void UpdateCollider(Collider2D collider)
    {

    }
}

