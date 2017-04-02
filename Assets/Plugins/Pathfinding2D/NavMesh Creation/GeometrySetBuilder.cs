using UnityEngine;
using System;
using System.Collections.Generic;

namespace NavGraph.Build
{
    /// <summary>
    /// Builds a geometry set out of supplied 2d Colliders.
    /// </summary>
    public class GeometrySetBuilder
    {
        /// <summary>
        /// The number of verts used to approximate a circle. Higher values lead to slower calculations, but higher accuracy.
        /// </summary>
        public int CircleVertCount
        {
            set
            {
                if (value < 3)
                    value = 3;
                anglePerCircleVert = (Mathf.PI * 2) / value;
                circleVertCount = value;
            }
            get { return circleVertCount; }
        }

        /// <summary>
        /// Determines the number of decimal places preserved of a Collider vert. Although higher values would lead to more accuracy,
        /// do to rounding errors while transfoming the verts, they can introduce systematic errors in later calculations.
        /// </summary>
        public int DecimalPlacesOfCoords { get { return decimalPlacesOfCoords; } set { decimalPlacesOfCoords = Mathf.Max(0, value); } }

        float anglePerCircleVert;
        int circleVertCount;
        int decimalPlacesOfCoords;

        public GeometrySetBuilder(int circleVertCount, int decimalPlacesOfCoords)
        {
            this.CircleVertCount = circleVertCount;
            this.DecimalPlacesOfCoords = decimalPlacesOfCoords;
        }

        public GeometrySet Build(IEnumerable<Collider2D> collider, int colliderCount)
        {
            //Assume 5% of the supplied colliders are edges.
            GeometrySet result = new GeometrySet((int)(colliderCount * 0.95f), (int)(colliderCount * 0.05f));

            foreach (Collider2D col in collider)
            {
                if (col == null)
                    continue;

                Type cTyp = col.GetType();

                if (cTyp == typeof(EdgeCollider2D))
                {
                    LoadEdgeColliderVerts((EdgeCollider2D)col, result);
                }
                else if (cTyp == typeof(BoxCollider2D))
                {
                    LoadBoxColliderVerts((BoxCollider2D)col, result);
                }
                else if (cTyp == typeof(CircleCollider2D))
                {
                    LoadCircleColliderVerts((CircleCollider2D)col, result);
                }
                else if (cTyp == typeof(PolygonCollider2D))
                {
                    LoadPolygonColliderVerts((PolygonCollider2D)col, result);
                }

            }
            return result;
        }

        private void RoundVerts(Vector2[] inOutVerts)
        {
            for (int iVert = 0; iVert < inOutVerts.Length; iVert++)
            {
                inOutVerts[iVert] = new Vector2(
                    (float)Math.Round(inOutVerts[iVert].x, decimalPlacesOfCoords),
                    (float)Math.Round(inOutVerts[iVert].y, decimalPlacesOfCoords)
                    );
            }
        }

        private void LoadBoxColliderVerts(BoxCollider2D collider, GeometrySet geometrySet)
        {
            Vector2 halfSize = collider.size / 2;
            Vector2[] verts = new Vector2[4];

            verts[0] = collider.transform.TransformPoint(halfSize + collider.offset);
            verts[1] = collider.transform.TransformPoint(new Vector2(halfSize.x, -halfSize.y) + collider.offset);
            verts[2] = collider.transform.TransformPoint(-halfSize + collider.offset);
            verts[3] = collider.transform.TransformPoint(new Vector2(-halfSize.x, halfSize.y) + collider.offset);

            RoundVerts(verts);
            geometrySet.AddPolygon(verts);
        }

        private void LoadCircleColliderVerts(CircleCollider2D collider, GeometrySet geometrySet)
        {
            Vector2[] verts = new Vector2[circleVertCount];
            for (int i = 0; i < circleVertCount; i++)
            {
                verts[i] = collider.transform.TransformPoint(new Vector2(collider.radius * Mathf.Sin(anglePerCircleVert * i) + collider.offset.x, collider.radius * Mathf.Cos(anglePerCircleVert * i) + collider.offset.y));
            }

            RoundVerts(verts);
            geometrySet.AddPolygon(verts);
        }

        private void LoadPolygonColliderVerts(PolygonCollider2D collider, GeometrySet geometrySet)
        {
            Matrix4x4 localToWorld = collider.transform.localToWorldMatrix;
            for (int iPath = 0; iPath < collider.pathCount; iPath++)
            {
                Vector2[] verts = collider.GetPath(iPath);
                for (int iVert = 0; iVert < verts.Length; iVert++)
                {
                    verts[iVert] = (localToWorld.MultiplyPoint(verts[iVert] + collider.offset));
                }
                RoundVerts(verts);
                geometrySet.AddPolygon(verts);
            }
        }

        private void LoadEdgeColliderVerts(EdgeCollider2D collider, GeometrySet geometrySet)
        {
            Matrix4x4 localToWorld = collider.transform.localToWorldMatrix;
            Vector2[] verts = new Vector2[collider.points.Length];
            for (int iVert = 0; iVert < collider.points.Length; iVert++)
            {
                verts[iVert] = localToWorld.MultiplyPoint(collider.points[iVert] + collider.offset);
            }
            RoundVerts(verts);
            geometrySet.AddEdge(verts);
        }
    }
}
