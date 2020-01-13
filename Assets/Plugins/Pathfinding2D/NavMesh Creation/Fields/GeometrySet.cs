using UnityEngine;
using System.Collections.Generic;
using System;

namespace NavGraph.Build
{
    /// <summary>
    /// A container that contains a list of polygons and edges, which where probably but not necessarily collected from Colliders.
    /// </summary>
    public class GeometrySet
    {
        public List<Vector2[]> Polygons { get; }
        public List<Vector2[]> Edges { get; }

        float anglePerCircleVert;
        int circleVertCount;

        public GeometrySet(int circleVertCount)
        {
            if (circleVertCount < 3)
                circleVertCount = 3;
            this.circleVertCount = circleVertCount;
            this.anglePerCircleVert = (Mathf.PI * 2) / circleVertCount;

            Polygons = new List<Vector2[]>(50);
            Edges = new List<Vector2[]>(10);
        }

        public void AddCollider(Collider2D col)
        {
            Type cTyp = col.GetType();

            if (cTyp == typeof(EdgeCollider2D))
            {
                AddEdgeCollider((EdgeCollider2D)col);
            }
            else if (cTyp == typeof(BoxCollider2D))
            {
                AddBoxCollider((BoxCollider2D)col);
            }
            else if (cTyp == typeof(CircleCollider2D))
            {
                AddCircleCollider((CircleCollider2D)col);
            }
            else if (cTyp == typeof(PolygonCollider2D))
            {
                AddPolygonCollider((PolygonCollider2D)col);
            }
        }

        public void AddBoxCollider(BoxCollider2D collider)
        {
            Vector2 halfSize = collider.size / 2;
            Vector2[] verts = new Vector2[4];

            verts[0] = collider.transform.TransformPoint(halfSize + collider.offset);
            verts[1] = collider.transform.TransformPoint(new Vector2(halfSize.x, -halfSize.y) + collider.offset);
            verts[2] = collider.transform.TransformPoint(-halfSize + collider.offset);
            verts[3] = collider.transform.TransformPoint(new Vector2(-halfSize.x, halfSize.y) + collider.offset);

            this.Polygons.Add(verts);
        }

        public void AddCircleCollider(CircleCollider2D collider)
        {
            Vector2[] verts = new Vector2[circleVertCount];
            for (int i = 0; i < circleVertCount; i++)
            {
                verts[i] = collider.transform.TransformPoint(new Vector2(collider.radius * Mathf.Sin(anglePerCircleVert * i) + collider.offset.x, collider.radius * Mathf.Cos(anglePerCircleVert * i) + collider.offset.y));
            }
            this.Polygons.Add(verts);
        }

        public void AddPolygonCollider(PolygonCollider2D collider)
        {
            Matrix4x4 localToWorld = collider.transform.localToWorldMatrix;
            for (int iPath = 0; iPath < collider.pathCount; iPath++)
            {
                Vector2[] verts = collider.GetPath(iPath);
                for (int iVert = 0; iVert < verts.Length; iVert++)
                {
                    verts[iVert] = (localToWorld.MultiplyPoint(verts[iVert] + collider.offset));
                }
                this.Polygons.Add(verts);
            }
        }

        public void AddEdgeCollider(EdgeCollider2D collider)
        {
            Matrix4x4 localToWorld = collider.transform.localToWorldMatrix;
            Vector2[] verts = new Vector2[collider.points.Length];
            for (int iVert = 0; iVert < collider.points.Length; iVert++)
            {
                verts[iVert] = localToWorld.MultiplyPoint(collider.points[iVert] + collider.offset);
            }
            this.Edges.Add(verts);
        }

        public void DebugVisualization(bool drawWithGizmos)
        {
            Color primaryColor = DefaultValues.Visualization_PrimaryColor;
            Color secondaryColor = DefaultValues.Visualization_SecondaryColor;
            Color highlightColor = DefaultValues.Visualization_HighlightColor;
            float indicatorSize = DefaultValues.Visualization_IndicatorSize;

            if (drawWithGizmos)
            {
                Gizmos.color = DefaultValues.Visualization_PrimaryColor;
            }

            //Draw polygons
            for (int iPoly = 0; iPoly < Polygons.Count; iPoly++)
            {
                for (int iVert = 0; iVert < Polygons[iPoly].Length - 1; iVert++)
                {
                    if (drawWithGizmos)
                    {
                        Gizmos.DrawLine(Polygons[iPoly][iVert], Polygons[iPoly][iVert + 1]);
                        DebugExtension.DrawCircle(Polygons[iPoly][iVert], Vector3.forward, secondaryColor, indicatorSize);
                    }
                    else
                    {
                        //DebugExtension.DebugArrow(polygonList[iPoly][iVert], polygonList[iPoly][iVert + 1] - polygonList[iPoly][iVert], primaryColor);
                        Debug.DrawLine(Polygons[iPoly][iVert], Polygons[iPoly][iVert + 1], primaryColor);
                        DebugExtension.DebugCircle(Polygons[iPoly][iVert], Vector3.forward, secondaryColor, indicatorSize);
                    }
                }

                if (drawWithGizmos)
                {
                    Gizmos.DrawLine(Polygons[iPoly][Polygons[iPoly].Length - 1], Polygons[iPoly][0]);
                    DebugExtension.DrawCircle(Polygons[iPoly][Polygons[iPoly].Length - 1], Vector3.forward, secondaryColor, indicatorSize);
                }
                else
                {
                    Debug.DrawLine(Polygons[iPoly][Polygons[iPoly].Length - 1], Polygons[iPoly][0], primaryColor);
                    DebugExtension.DebugCircle(Polygons[iPoly][Polygons[iPoly].Length - 1], Vector3.forward, secondaryColor, indicatorSize);
                }
            }

            //Draw edges
            for (int iEdge = 0; iEdge < Edges.Count; iEdge++)
            {
                for (int iVert = 0; iVert < Edges[iEdge].Length - 1; iVert++)
                {
                    if (drawWithGizmos)
                    {
                        Gizmos.DrawLine(Polygons[iEdge][iVert], Polygons[iEdge][iVert + 1]);
                        DebugExtension.DrawCircle(Polygons[iEdge][iVert], Vector3.forward, secondaryColor, indicatorSize);
                    }
                    else
                    {
                        Debug.DrawLine(Edges[iEdge][iVert], Edges[iEdge][iVert + 1], primaryColor);
                        DebugExtension.DebugCircle(Edges[iEdge][iVert], Vector3.forward, secondaryColor, indicatorSize);
                    }
                }
            }
        }
    }
}
