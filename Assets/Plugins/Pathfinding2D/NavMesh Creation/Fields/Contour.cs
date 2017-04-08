using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NavGraph.Build
{
    /// <summary>
    /// Contains a polygon.
    /// </summary>
    [Serializable]
    public class Contour : IEnumerable<Vector2>, IVisualizable
    {
        public int VertexCount { get { return verticies.Count; } }
        public Bounds Bounds { get { if (!areBoundsValid) CalcBounds(); return bounds; } }
        public bool ContainsNoVerts { get { return verticies.Count == 0; } }

        [SerializeField]
        List<Vector2> verticies;
        [SerializeField]
        private Bounds bounds;
        [SerializeField]
        private bool areBoundsValid;

        public Contour(params Vector2[] verticies)
        {
            this.verticies = new List<Vector2>(verticies);
        }

        public Contour(List<Vector2> verticies)
        {
            this.verticies = new List<Vector2>();
            this.verticies.AddRange(verticies);
        }

        public void AddVertex(Vector2 v)
        {
            verticies.Add(v);

            if (!areBoundsValid)
                CalcBounds();
            bounds.max = Vector2.Max(bounds.max, v);
            bounds.min = Vector2.Min(bounds.min, v);
        }

        public Vector2 this[int key]
        {
            get { return verticies[key]; }
        }

        public void Optimize(float nodeMergeDist, float maxEdgeDeviation)
        {
            Vector2 prevVert = verticies[verticies.Count - 1];
            Vector2 prevPrevVert = verticies[verticies.Count - 2];
            float srqMergeDist = nodeMergeDist * nodeMergeDist;

            for (int iVert = 0; iVert < verticies.Count && verticies.Count > 3; iVert++)
            {
                //When the distance between this vertex and the previous vertex is below the threshold, remove it.
                if ((prevVert - verticies[iVert]).sqrMagnitude <= srqMergeDist)
                {
                    //Remove vertex
                    RemoveVertexAt(iVert);
                    iVert--;
                }
                else
                {
                    //Check if the previous vertex lies on the edge between the vertex before it and this vertex.
                    //If the angle between the line from the previous vertex to the vertex before the previous vertex
                    //and the line between the previous vertex and this one is greater then the threshold, remove the previous vertex.
                    Vector2 nA = prevPrevVert - prevVert;
                    Vector2 nB = verticies[iVert] - prevVert;
                    float angle = Vector2.Angle(nA, nB);

                    if (angle >= 180 - maxEdgeDeviation)
                    {
                        //Remove vertex
                        RemoveVertexAt((verticies.Count + (iVert - 1)) % verticies.Count);

                        //When the removed vertex doesn't come before the current one, we don't have to adjust the pointer.
                        if (iVert > 0)
                            iVert--;
                        prevVert = verticies[iVert];
                    }
                    else
                    {
                        //Update pointer to previous vertices.
                        prevPrevVert = prevVert;
                        prevVert = verticies[iVert];
                    }
                }
            }
        }

        public void RemoveVertexAt(int pos)
        {
            if (pos < 0 || pos >= verticies.Count || verticies.Count == 3)// The contour must at least contain 3 vertices.
                throw new Exception("Tried to remove vertex failed. Remove index = " + pos + ", VertexCount = " + verticies.Count);
            verticies.RemoveAt(pos);
            areBoundsValid = false;
        }

        public bool IsSolid()
        {
            return CalcArea() <= 0;
        }

        public float CalcArea()
        {
            float area = 0;
            int j = verticies.Count - 1;

            for (int i = 0; i < verticies.Count; i++)
            {
                area = area + (verticies[j].x + verticies[i].x) * (verticies[j].y - verticies[i].y);
                j = i;
            }
            return area / 2 * -1;
        }

        public Vector2[] GetVertexArray()
        {
            return verticies.ToArray();
        }

        public Vector3[] GetVertex3dArray()
        {
            Vector3[] result = new Vector3[verticies.Count];
            for (int iVert = 0; iVert < verticies.Count; iVert++)
                result[iVert] = verticies[iVert];
            return result;
        }

        public IEnumerator<Vector2> GetEnumerator()
        {
            return ((IEnumerable<Vector2>)verticies).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Vector2>)verticies).GetEnumerator();
        }

        private void CalcBounds()
        {
            areBoundsValid = true;
            bounds.min = verticies[0];
            bounds.max = verticies[0];
            for (int iVert = 1; iVert < verticies.Count; iVert++)
            {
                bounds.max = Vector2.Max(bounds.max, verticies[iVert]);
                bounds.min = Vector2.Min(bounds.min, verticies[iVert]);
            }
        }

        public void DebugVisualization(bool drawWithGizmos)
        {
            DebugVisualization(drawWithGizmos, DefaultValues.Visualization_PrimaryColor,
                DefaultValues.Visualization_SecondaryColor,
                DefaultValues.Visualization_HighlightColor,
                DefaultValues.Visualization_IndicatorSize);
        }

        public void DebugVisualization(bool drawWithGizmos, Color primaryColor, Color secondaryColor)
        {
            DebugVisualization(drawWithGizmos, primaryColor, secondaryColor, DefaultValues.Visualization_HighlightColor, DefaultValues.Visualization_IndicatorSize);
        }

        public void DebugVisualization(bool drawWithGizmos, Color primaryColor, Color secondaryColor, float indicatorSize)
        {
            DebugVisualization(drawWithGizmos, primaryColor, secondaryColor, DefaultValues.Visualization_HighlightColor, indicatorSize);
        }

        public void DebugVisualization(bool drawWithGizmos, Color primaryColor, Color secondaryColor, Color highlightColor, float indicatorSize)
        {
            if (drawWithGizmos)
                Gizmos.color = primaryColor;

            Vector2 prev = verticies[verticies.Count - 1];
            foreach (Vector2 vert in verticies)
            {
                if (drawWithGizmos)
                {
                    Gizmos.DrawLine(prev, vert);
                }
                else
                {
                    Debug.DrawLine(prev, vert, primaryColor);
                }
                prev = vert;
            }
        }
    }
}
