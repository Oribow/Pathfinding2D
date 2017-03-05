using UnityEngine;
using System.Collections.Generic;
using System;

namespace Pathfinding2d.NavDataGeneration
{
    /// <summary>
    /// A container that contains a list of polygons and edges, which where probably but not necessarily collected from Colliders.
    /// </summary>
    internal class GeometrySet : IVisualizable
    {
        public List<Vector2[]> Polygons { get { return polygonList; } }
        public List<Vector2[]> Edges { get { return edgeList; } }

        List<Vector2[]> polygonList;
        List<Vector2[]> edgeList;

        public GeometrySet(int polygonCapacity, int edgeCapacity)
        {
            polygonList = new List<Vector2[]>(polygonCapacity);
            edgeList = new List<Vector2[]>(edgeCapacity);
        }

        public void AddEdge(List<Vector2> verts)
        {
            edgeList.Add(Array.ConvertAll(verts.ToArray(), (item) => (Vector2)item));
        }

        public void AddPolygon(List<Vector2> verts)
        {
            polygonList.Add(Array.ConvertAll(verts.ToArray(), (item) => (Vector2)item));
        }

        public void AddEdge(Vector2[] verts)
        {
            edgeList.Add(verts);
        }

        public void AddPolygon(Vector2[] verts)
        {
            polygonList.Add(verts);
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
            DebugVisualization(drawWithGizmos, primaryColor, secondaryColor,
                DefaultValues.Visualization_HighlightColor,
                DefaultValues.Visualization_IndicatorSize);
        }

        public void DebugVisualization(bool drawWithGizmos, Color primaryColor, Color secondaryColor, float indicatorSize)
        {
            DebugVisualization(drawWithGizmos, primaryColor, secondaryColor,
               DefaultValues.Visualization_HighlightColor, indicatorSize);
        }

        public void DebugVisualization(bool drawWithGizmos, Color primaryColor, Color secondaryColor, Color highlightColor, float indicatorSize)
        {
            if (drawWithGizmos)
            {
                Gizmos.color = DefaultValues.Visualization_PrimaryColor;
            }

            //Draw polygons
            for (int iPoly = 0; iPoly < polygonList.Count; iPoly++)
            {
                for (int iVert = 0; iVert < polygonList[iPoly].Length - 1; iVert++)
                {
                    if (drawWithGizmos)
                    {
                        Gizmos.DrawLine(polygonList[iPoly][iVert], polygonList[iPoly][iVert + 1]);
                        DebugExtension.DrawCircle(polygonList[iPoly][iVert], Vector3.forward, secondaryColor, indicatorSize);
                    }
                    else
                    {
                        Debug.DrawLine(polygonList[iPoly][iVert], polygonList[iPoly][iVert + 1], primaryColor);
                        DebugExtension.DebugCircle(polygonList[iPoly][iVert], Vector3.forward, secondaryColor, indicatorSize);
                    }
                }

                if (drawWithGizmos)
                {
                    Gizmos.DrawLine(polygonList[iPoly][polygonList[iPoly].Length - 1], polygonList[iPoly][0]);
                    DebugExtension.DrawCircle(polygonList[iPoly][polygonList[iPoly].Length - 1], Vector3.forward, secondaryColor, indicatorSize);
                }
                else
                {
                    Debug.DrawLine(polygonList[iPoly][polygonList[iPoly].Length - 1], polygonList[iPoly][0], primaryColor);
                    DebugExtension.DebugCircle(polygonList[iPoly][polygonList[iPoly].Length - 1], Vector3.forward, secondaryColor, indicatorSize);
                }
            }

            //Draw edges
            for (int iEdge = 0; iEdge < edgeList.Count; iEdge++)
            {
                for (int iVert = 0; iVert < edgeList[iEdge].Length - 1; iVert++)
                {
                    if (drawWithGizmos)
                    {
                        Gizmos.DrawLine(polygonList[iEdge][iVert], polygonList[iEdge][iVert + 1]);
                        DebugExtension.DrawCircle(polygonList[iEdge][iVert], Vector3.forward, secondaryColor, indicatorSize);
                    }
                    else
                    {
                        Debug.DrawLine(edgeList[iEdge][iVert], edgeList[iEdge][iVert + 1], primaryColor);
                        DebugExtension.DebugCircle(edgeList[iEdge][iVert], Vector3.forward, secondaryColor, indicatorSize);
                    }
                }
            }
        }
    }
}
