using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pathfinding2d
{
    /// <summary>
    /// Contains all information needed to navigate.
    /// </summary>
    [Serializable]
    public class NavigationData2D : ScriptableObject
    {
        const float mapPointMaxDeviation = 3;
        const float mapPointInstantAcceptDeviation = 0.01f;

        //identifiers
        public new string name;
        public int version;

        public NavNode[] nodes; // sorted by x Value. Min -> Max
        public NavAgentGroundWalkerSettings navAgentSettings;

        public bool SamplePoint(Vector2 point, out NavPosition navPosition)
        {
            NavNode map_cNavNode;
            int cVertIndex;
            float map_minDist = float.MaxValue;
            float dist;
            Vector2 cPoint;
            navPosition = new NavPosition();

            for (int iNavNode = 0; iNavNode < nodes.Length; iNavNode++)
            {
                map_cNavNode = nodes[iNavNode];

                //Extended bounds test
                if (map_cNavNode.bounds.min.x - mapPointMaxDeviation > point.x || map_cNavNode.bounds.max.x + mapPointMaxDeviation < point.x
                || map_cNavNode.bounds.min.y - mapPointMaxDeviation > point.y || map_cNavNode.bounds.max.y + mapPointMaxDeviation < point.y)
                {
                    //Failed test
                    continue;
                }

                if (map_cNavNode.isClosed && map_cNavNode.Contains(point))
                {
                    //maybe later check children, not implemented though
                    return false;
                }

                if (map_cNavNode.TryFindClosestPointOnContour(point, out dist, out cPoint, out cVertIndex))
                {
                    if (dist < map_minDist)
                    {
                        navPosition.navVertIndex = cVertIndex;
                        navPosition.navNodeIndex = iNavNode;
                        navPosition.navPoint = cPoint;
                        if (dist <= mapPointInstantAcceptDeviation)
                        {
                            return true;
                        }
                        map_minDist = dist;
                    }
                }
            }

            return map_minDist != float.MaxValue;
        }

#if UNITY_EDITOR
        public void SaveToAsset()
        {
            string path = EditorUtility.SaveFilePanel("Save NavData2D", "Assets", "NavData2D", "asset");
            if (path == null || path.Length == 0)
                return;
            path = path.Substring(path.IndexOf("Assets"));
            Debug.Log(path);
            AssetDatabase.CreateAsset(this, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = this;
        }
#endif
    }

    /// <summary>
    /// A series of edges, which a character could traverse just by walking.
    /// </summary>
    [Serializable]
    public class NavNode : ISerializationCallbackReceiver, IVisualizable
    {
        const float maxDeviationInside = 0.1f;
        const float maxDeviationOutside = 0.001f;

        public Bounds bounds;
        public bool isClosed;
        public int hierachyIndex; // 0 = hole, 1 = solid, 2 = hole, 3 = solid, ...

        public bool IsSolid { get { return hierachyIndex % 2 == 0; } }

        public IOffNodeLink[] links;
        public NavVert[] verts;

        [SerializeField, HideInInspector]
        byte[] serializableLinks;

        public NavNode(NavVert[] verts, Bounds bounds, bool isClosed, int hierachyIndex)
        {
            this.verts = verts;
            this.bounds = bounds;
            this.isClosed = isClosed;
            this.hierachyIndex = hierachyIndex;
            links = new IOffNodeLink[0];
        }

        public bool Contains(Vector2 point)
        {
            Debug.Assert(isClosed);

            if (!bounds.Contains(point))
            {
                //Bound test failed
                return false;
            }

            bool inside = false;
            NavVert cVert = verts[verts.Length - 1];
            for (int iEdge = 0; iEdge < verts.Length; iEdge++)
            {
                if ((verts[iEdge].PointB.y > point.y) != (cVert.PointB.y > point.y) &&
                    point.x < (cVert.PointB.x - verts[iEdge].PointB.x) * (point.y - verts[iEdge].PointB.y) / (cVert.PointB.y - verts[iEdge].PointB.y) + verts[iEdge].PointB.x)
                {
                    inside = !inside;
                }
                cVert = verts[iEdge];
            }
            return inside;
        }

        public bool TryFindClosestPointOnContour(Vector2 point, out float distance, out Vector2 nearestPoint)
        {
            int vert;
            return TryFindClosestPointOnContour(point, out distance, out nearestPoint, out vert);
        }

        public bool TryFindClosestPointOnContour(Vector2 point, out float distance, out Vector2 nearestPoint, out int nearestEdgeIndex)
        {
            distance = float.MaxValue;
            nearestEdgeIndex = 0;
            nearestPoint = Vector2.zero;
            NavVert cVert = (isClosed) ? verts[verts.Length - 1] : verts[0];
            for (int iEdge = isClosed ? 0 : 1; iEdge < verts.Length; iEdge++)
            {
                float lineSide = Mathf.Sign((verts[iEdge].PointB.x - cVert.PointB.x) * (point.y - cVert.PointB.y) - (verts[iEdge].PointB.y - cVert.PointB.y) * (point.x - cVert.PointB.x));
                if (lineSide == 0)
                {
                    distance = 0;
                    nearestEdgeIndex = iEdge;
                    nearestPoint = point;
                    return true;
                }
                if (lineSide == 1)
                {
                    cVert = verts[iEdge];
                    continue;
                }

                //Point is on right side. Now calculate distance.
                Vector2 AP = point - cVert.PointB;       //Vector from A to P   
                Vector2 AB = verts[iEdge].PointB - cVert.PointB;
                float ABAPproduct = Vector2.Dot(AP, AB);    //The DOT product of a_to_p and a_to_b     
                float dis = Mathf.Clamp(ABAPproduct / AB.sqrMagnitude, 0, 1); //The normalized "distance" from a to your closest point  

                AP = AB * dis + cVert.PointB;
                dis = (AP - point).sqrMagnitude;
                if (distance > dis)
                {
                    distance = dis;
                    nearestPoint = AP;
                    nearestEdgeIndex = iEdge;
                }
                cVert = verts[iEdge];
            }
            if (distance == float.MaxValue)
            {
                return false;
            }
            distance = Mathf.Sqrt(distance);
            return true;
        }

        public void OnBeforeSerialize()
        {
            if (links == null)
                serializableLinks = null;
            else
            {
                SurrogateSelector selector = new SurrogateSelector();
                selector.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All), new Vector2SerializationSurrogate());
                BinaryFormatter bf = new BinaryFormatter(selector, new StreamingContext(StreamingContextStates.All));
                using (MemoryStream ms = new MemoryStream())
                {
                    bf.Serialize(ms, links);
                    serializableLinks = ms.ToArray();
                }
            }
        }

        public void OnAfterDeserialize()
        {
            if (serializableLinks == null || serializableLinks.Length == 0)
                return;

            using (MemoryStream memStream = new MemoryStream())
            {
                SurrogateSelector selector = new SurrogateSelector();
                selector.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All), new Vector2SerializationSurrogate());
                BinaryFormatter bf = new BinaryFormatter(selector, new StreamingContext(StreamingContextStates.All));
                memStream.Write(serializableLinks, 0, serializableLinks.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                links = (IOffNodeLink[])bf.Deserialize(memStream);
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
            if (verts.Length < 2)
                return;

            Gizmos.color = primaryColor;
            for (int iVert = 0; iVert < verts.Length - 1; iVert++)
            {
                if (verts[iVert].PointB == verts[iVert + 1].PointB)
                {
                    Debug.Log("Node has edge with zero length. " + verts[iVert].PointB + ", totalVertCount = " + verts.Length);
                    if (drawWithGizmos)
                        DebugExtension.DrawPoint(verts[iVert].PointB, Color.magenta);
                    else
                        DebugExtension.DebugPoint(verts[iVert].PointB, Color.magenta);
                }
                if (drawWithGizmos)
                {
                    Gizmos.DrawLine(verts[iVert].PointB, verts[iVert + 1].PointB);
                    DebugExtension.DrawCircle(verts[iVert].PointB, Vector3.forward, secondaryColor, indicatorSize);
                }
                else
                {
                    Debug.DrawLine(verts[iVert].PointB, verts[iVert + 1].PointB, primaryColor);
                    DebugExtension.DebugCircle(verts[iVert].PointB, Vector3.forward, secondaryColor, indicatorSize);
                }
            }
            if (isClosed)
            {
                if (drawWithGizmos)
                {
                    Gizmos.DrawLine(verts[verts.Length - 1].PointB, verts[0].PointB);
                } else
                {
                    Debug.DrawLine(verts[verts.Length - 1].PointB, verts[0].PointB, primaryColor);
                }
            }
            DebugExtension.DebugCircle(verts[verts.Length - 1].PointB, Vector3.forward, secondaryColor, indicatorSize);
        }
    }

    /// <summary>
    /// Basically an enhanced Vector2, with some information about the edge.
    /// </summary>
    [Serializable]
    public class NavVert
    {
        public Vector2 PointB { get { return pointB; } }

        public float angleABC;
        public float slopeAngleBC;
        public float distanceBC;

        [SerializeField]
        Vector2 pointB; // a -> b -> c

        public int[] linkIndex;

        public NavVert(Vector2 point, float angleABC, float slopeAngleBC, float distanceBC)
        {
            this.pointB = point;
            this.angleABC = angleABC;
            this.slopeAngleBC = slopeAngleBC;
            this.distanceBC = distanceBC;
        }

        public NavVert(Vector2 point)
        {
            this.pointB = point;
        }
    }
}
