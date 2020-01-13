using UnityEngine;
using System.Collections.Generic;
using System;
using Utility.Polygon2D;

namespace NavGraph.Build
{
    /// <summary>
    /// A container that contains a list of polygons and edges, which where probably but not necessarily collected from Colliders.
    /// </summary>
    public class PolygonSet
    {
        public List<Polygon> Polygons { get; }
        public List<Vector2[]> Edges { get; }

        float anglePerCircleVert;
        int circleVertCount;

        public PolygonSet(int circleVertCount)
        {
            if (circleVertCount < 3)
                circleVertCount = 3;
            this.circleVertCount = circleVertCount;
            this.anglePerCircleVert = (Mathf.PI * 2) / circleVertCount;

            Polygons = new List<Polygon>(50);
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
                AddPolygon(Polygon.FromBoxCollider2D((BoxCollider2D)col));
            }
            else if (cTyp == typeof(CircleCollider2D))
            {
                AddPolygon(Polygon.FromCircleCollider2D((CircleCollider2D)col, circleVertCount, anglePerCircleVert));
            }
            else if (cTyp == typeof(PolygonCollider2D))
            {
                PolygonCollider2D pCol = (PolygonCollider2D)col;
                if (Polygons.Capacity < Polygons.Count + pCol.pathCount)
                {
                    Polygons.Capacity = Polygons.Count + pCol.pathCount;
                }
                for (int iPath = 0; iPath < pCol.pathCount; iPath++)
                {
                    AddPolygon(Polygon.FromPolygonCollider2D(pCol, iPath));
                }
            }
        }

        private void AddEdgeCollider(EdgeCollider2D collider)
        {
            Matrix4x4 localToWorld = collider.transform.localToWorldMatrix;
            Vector2[] verts = new Vector2[collider.points.Length];
            for (int iVert = 0; iVert < collider.points.Length; iVert++)
            {
                verts[iVert] = localToWorld.MultiplyPoint(collider.points[iVert] + collider.offset);
            }
            this.Edges.Add(verts);
        }

        private void AddPolygon(Polygon newPoly)
        {
            // try to merge the new polygon with the existing once
            foreach (var poly in this.Polygons)
            {
                Contour[] result;
                var resultType = PolygonClipper.Compute(poly.hull, newPoly.hull, PolygonClipper.BoolOpType.UNION, out result);

                switch (resultType) {
                    case PolygonClipper.ResultType.NoOverlap:
                        continue;

                    case PolygonClipper.ResultType.ClipperPolygonFullyOverlapsSourcePolygon:
                        // great, now test, if clipper polygon holes will be shrinked by the source polygons solid area

                        break;
                    case PolygonClipper.ResultType.SourcePolygonFullOverlapsClipperPolygon:
                        // great, now test if source polygons holes will be shrinked by the clipper polygons solid area
                        foreach (var newContour in result)
                        {
                            if (newContour.IsAHole())
                            {

                            }
                            else { 
                                // this is the new hull
                                poly.hull
                            }
                        }

                        break;

                    case PolygonClipper.ResultType.Overlap:
                        // continue with results, but check holes first

                        break;
                }
            }
        }

        private

        /*
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
        }*/
    }
}
