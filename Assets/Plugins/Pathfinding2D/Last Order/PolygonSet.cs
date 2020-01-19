using UnityEngine;
using System.Collections.Generic;
using System;
using ClipperLib;

#if use_int32
  using cInt = Int32;
#else
using cInt = System.Int64;
#endif

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
        public int floatToIntMult;

        Clipper clipper;

        public PolygonSet(int circleVertCount, int floatToIntMult)
        {
            if (circleVertCount < 3)
                circleVertCount = 3;
            this.circleVertCount = circleVertCount;
            this.anglePerCircleVert = (Mathf.PI * 2) / circleVertCount;
            this.floatToIntMult = floatToIntMult;

            Polygons = new List<Polygon>(50);
            Edges = new List<Vector2[]>(10);

            clipper = new Clipper();
            clipper.ReverseSolution = true;
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
                AddPolygon(PolygonFromBoxCollider2D((BoxCollider2D)col));
            }
            else if (cTyp == typeof(CircleCollider2D))
            {
                AddPolygon(PolygonFromCircleCollider2D((CircleCollider2D)col));
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
                    AddPolygon(PolygonFromPolygonCollider2D(pCol, iPath));
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
            for (int iPoly = 0; iPoly < Polygons.Count; iPoly++)
            {
                var poly = Polygons[iPoly];
                // simple bounds check first
                if (!poly.BoundingRect.Overlaps(newPoly.BoundingRect))
                {
                    continue;
                }

                clipper.Clear();
                newPoly.AddToClipper(clipper, PolyType.ptClip);
                poly.AddToClipper(clipper, PolyType.ptSubject);

                PolyTree polyTree = new PolyTree();
                clipper.Execute(ClipType.ctUnion, polyTree);

                if (polyTree.ChildCount > 1)
                {
                    // bounds overlap, but no actual intersection is happening
                    continue;
                }
                var hullNode = polyTree.Childs[0];
                bool holeNodeHasChild = false;
                foreach (var holeNode in hullNode.Childs)
                {
                    if (holeNode.ChildCount != 0)
                    {
                        holeNodeHasChild = true;
                        break;
                    }
                }
                if (holeNodeHasChild)
                {
                    continue;
                }

                // tree can now contains 1 outer polygon and any number of holes.
                // we cant know which outer polygon corresponses to the outer polygon

                //overwrite newPoly

                newPoly.hull.SetVerticies(hullNode.Contour);

                newPoly.holes = new List<Contour>(hullNode.ChildCount);
                foreach (var holeNode in hullNode.Childs)
                {
                    newPoly.holes.Add(new Contour(holeNode.Contour));
                }
                newPoly.UpdateBounds();

                // delete poly
                if (iPoly < Polygons.Count - 1)
                {
                    Polygons[iPoly] = Polygons[Polygons.Count - 1];
                }
                Polygons.RemoveAt(Polygons.Count - 1);
                iPoly--;
            }
            Polygons.Add(newPoly);
        }

        public Vector2 IntPointToVector2(IntPoint intPoint)
        {
            return new Vector2(intPoint.x / (float)floatToIntMult, intPoint.y / (float)floatToIntMult);
        }

        public IntPoint Vector2ToIntPoint(Vector2 v)
        {
            return new IntPoint(Mathf.RoundToInt(v.x * floatToIntMult), Mathf.RoundToInt(v.y * floatToIntMult));
        }

        public cInt FloatToCInt(float f)
        {
            return Mathf.RoundToInt(f * floatToIntMult);
        }

        private Polygon PolygonFromBoxCollider2D(BoxCollider2D collider)
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

            return new Polygon(new Contour(verts));
        }

        private Polygon PolygonFromCircleCollider2D(CircleCollider2D collider)
        {
            List<IntPoint> verts = new List<IntPoint>(circleVertCount);
            for (int i = 0; i < circleVertCount; i++)
            {
                var v = collider.transform.TransformPoint(new Vector2(collider.radius * Mathf.Sin(anglePerCircleVert * i) + collider.offset.x, collider.radius * Mathf.Cos(anglePerCircleVert * i) + collider.offset.y));
                verts.Add(Vector2ToIntPoint(v));
            }

            return new Polygon(new Contour(verts));
        }

        private Polygon PolygonFromPolygonCollider2D(PolygonCollider2D collider, int pathIndex)
        {
            Matrix4x4 localToWorld = collider.transform.localToWorldMatrix;
            var path = collider.GetPath(pathIndex);
            List<IntPoint> verts = new List<IntPoint>(path.Length);
            for (int iVert = 0; iVert < path.Length; iVert++)
            {
                verts.Add(Vector2ToIntPoint(localToWorld.MultiplyPoint(path[iVert] + collider.offset)));
            }
            return new Polygon(new Contour(verts));
        }
    }
}
