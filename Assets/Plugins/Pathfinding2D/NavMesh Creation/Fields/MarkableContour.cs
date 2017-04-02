using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace NavGraph.Build
{
    /// <summary>
    /// A version of the Contour class, in which it is possible to mark edges as "not walkable"
    /// </summary>
    public class MarkableContour : IEnumerable<PointNode>
    {
        public readonly Bounds bounds;
        public readonly bool isSolid;
        public readonly int pointNodeCount;

        public bool IsEmpty { get { return pointNodeCount == 0; } }

        public readonly PointNode firstPoint;

        public MarkableContour(Contour contour, bool isSolid)
        {
            bounds = contour.Bounds;
            this.isSolid = isSolid;

            pointNodeCount = contour.VertexCount;
            PointNode cSeg = new PointNode(contour[0]);
            firstPoint = cSeg;
            for (int iVert = 1; iVert < contour.VertexCount; iVert++)
            {
                cSeg.Next = new PointNode(contour[iVert]);
                cSeg.Next.Previous = cSeg;
                cSeg = cSeg.Next;
            }
            cSeg.Next = firstPoint;
            firstPoint.Previous = cSeg;
        }

        public MarkableContour(SerializableMarkableContour src)
        {
            bounds = src.bounds;
            isSolid = src.isSolid;

            pointNodeCount = src.points.Length;

            PointNode cSeg = new PointNode(src.points[0], null);
            PointNode prevSeg = cSeg;
            firstPoint = cSeg;
            for (int iVert = 1; iVert < pointNodeCount; iVert++)
            {
                cSeg = new PointNode(src.points[iVert], prevSeg);
                prevSeg.SetNextNodeNoRecalculation(cSeg);
                prevSeg = cSeg;
            }
            cSeg.SetNextNodeNoRecalculation(firstPoint);
            firstPoint.SetPrevNodeNoRecalculation(cSeg);
        }

        public bool Contains(Vector2 point)
        {
            if (!bounds.Contains(point))
                return false;

            bool inside = false;
            PointNode cNode = firstPoint;
            for (int i = 0; i < pointNodeCount; i++)
            {
                if ((cNode.pointB.y > point.y) != (cNode.pointA.y > point.y) &&
                     point.x < (cNode.pointA.x - cNode.pointB.x) * (point.y - cNode.pointB.y) / (cNode.pointA.y - cNode.pointB.y) + cNode.pointB.x)
                {
                    inside = !inside;
                }
                cNode = cNode.Next;
            }
            return inside;
        }

        public Vector2 ClosestPointOnContour(Vector2 point, out float distance, out Vector2 tangent)
        {
            distance = float.MaxValue;
            Vector2 closestPoint = Vector2.zero; //dummy value
            tangent = Vector2.zero;
            int edgeCount = pointNodeCount;
            PointNode pn = firstPoint;

            for (int i = 0; i < edgeCount; i++, pn = pn.Next)
            {
                //Check if point lies on the outside of the line
                float lineSide = Mathf.Sign((pn.pointC.x - pn.pointB.x) * (point.y - pn.pointB.y) - (pn.pointC.y - pn.pointB.y) * (point.x - pn.pointB.x));
                if (lineSide == 0)
                {
                    tangent = pn.tangentBC;
                    distance = 0;
                    return point;
                }
                if (lineSide == 1)
                    continue;

                //Point is on right side. Now calculate distance.
                Vector2 AP = point - pn.pointB;       //Vector from A to P   
                Vector2 AB = pn.pointC - pn.pointB;
                float ABAPproduct = Vector2.Dot(AP, AB);    //The DOT product of a_to_p and a_to_b     
                float dis = Mathf.Clamp(ABAPproduct / AB.sqrMagnitude, 0, 1); //The normalized "distance" from a to your closest point  

                AP = AB * dis + pn.pointB;
                dis = (AP - point).sqrMagnitude;
                if (distance > dis)
                {
                    distance = dis;
                    closestPoint = AP;
                    tangent = pn.tangentBC;
                }
            }
            distance = Mathf.Sqrt(distance);
            return closestPoint;
        }

        public IEnumerator<PointNode> GetEnumerator()
        {
            return new PointNodeEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new PointNodeEnumerator(this);
        }

        class PointNodeEnumerator : IEnumerator<PointNode>
        {
            MarkableContour target;
            int cIndex = 0;
            PointNode cSeg;

            public PointNodeEnumerator(MarkableContour target)
            {
                this.target = target;
                cSeg = target.firstPoint;
            }

            public PointNode Current
            {
                get
                {
                    return cSeg;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return cSeg;
                }
            }

            public void Dispose()
            {
                target = null;
                cSeg = null;
            }

            public bool MoveNext()
            {
                if (cIndex >= target.pointNodeCount)
                {
                    return false;
                }
                else
                {
                    cIndex++;
                    cSeg = cSeg.Next;
                    return true;
                }
            }

            public void Reset()
            {
                cIndex = 0;
                cSeg = target.firstPoint;
            }
        }
    }

    /// <summary>
    /// Holds a vertex and a reference to its neighbors. Holds also some cached data about the edge, that the
    /// point is forming with its neighbors. A->[B]->C The edge described is between B and C. The Vertex
    /// that a instance hold is treated as B.
    /// </summary>
    public class PointNode : IVisualizable
    {
        public Vector2 pointB;
        public Vector2 pointC { get { return next.pointB; } }
        public Vector2 pointA { get { return prev.pointB; } }
        public PointNode Previous { get { return prev; } set { prev = value; if (next != null && prev != null) { CalcAngle(); PrecomputeVars(); } } }
        public PointNode Next { get { return next; } set { next = value; if (next != null && prev != null) { CalcAngle(); PrecomputeVars(); } } }
        public ObstructedSegment FirstObstructedSegment { get { return obstructedSegment; } }

        //Precomputed information
        public float angle; // in rads
        public float distanceBC;
        public Vector2 tangentBC;
        public bool isPointWalkable;

        PointNode prev;
        PointNode next;
        ObstructedSegment obstructedSegment;

        public PointNode(Vector2 point)
        {
            this.pointB = point;
            angle = -1;
        }

        public PointNode(SerializablePointNode src, PointNode prev)
        {
            angle = src.angle;
            distanceBC = src.distanceBC;
            tangentBC = src.tangentBC;
            pointB = src.pointB;
            isPointWalkable = src.isPointWalkable;
            this.prev = prev;

            ObstructedSegment prevObstr = null;
            ObstructedSegment cSeg = null;
            for (int i = src.obstructedSegments.Length - 1; i >= 0; i--)
            {
                cSeg = new ObstructedSegment(src.obstructedSegments[i], prevObstr);
                prevObstr = cSeg;
            }
            obstructedSegment = cSeg;
        }

        public void AddObstruction(float start, float end)
        {
            if (start == end)
                return;
            if (start > end)
                Debug.Log("Violation of order!");
            ObstructedSegment cSeg = obstructedSegment;
            ObstructedSegment prevSeg = null;

            while (cSeg != null)
            {
                if (cSeg.start > end)
                {
                    ObstructedSegment newSeg = new ObstructedSegment(start, end);
                    newSeg.next = cSeg;
                    if (prevSeg != null)
                        prevSeg.next = newSeg;
                    else
                        obstructedSegment = newSeg;
                    return;
                }
                else if (cSeg.end >= start)
                {
                    start = Mathf.Min(start, cSeg.start);
                    end = Mathf.Max(end, cSeg.end);
                    if (prevSeg != null)
                        prevSeg.next = cSeg.next;
                    cSeg = cSeg.next;
                }
                else
                {
                    prevSeg = cSeg;
                    cSeg = cSeg.next;
                }
            }

            cSeg = new ObstructedSegment(start, end);
            if (prevSeg != null)
                prevSeg.next = cSeg;
            else
                obstructedSegment = cSeg;
        }

        public void MarkPointNotWalkable()
        {
            isPointWalkable = false;
        }

        public bool IsPointWalkable()
        {
            return isPointWalkable;
        }

        public void SetNextNodeNoRecalculation(PointNode next)
        {
            this.next = next;
        }

        public void SetPrevNodeNoRecalculation(PointNode prev)
        {
            this.prev = prev;
        }

        private void CalcAngle()
        {
            Vector2 nA = pointA - pointB;
            Vector2 nB = pointC - pointB;
            angle = Vector2.Angle(nA, nB) * Mathf.Deg2Rad;
            if (Vector3.Cross(nA, nB).z < 0)
                angle = Mathf.PI * 2 - angle;
        }

        private void PrecomputeVars()
        {
            distanceBC = Vector2.Distance(pointB, pointC);
            tangentBC = (pointC - pointB) / distanceBC;
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

            if (!isPointWalkable)
            {
                if (drawWithGizmos)
                    DebugExtension.DrawCircle(pointB, Vector3.forward, Color.red, indicatorSize);
                else
                    DebugExtension.DebugCircle(pointB, Vector3.forward, Color.red, indicatorSize);
            }

            if (drawWithGizmos)
                Gizmos.DrawLine(pointB, pointC);
            else
                Debug.DrawLine(pointB, pointC, primaryColor);

            ObstructedSegment pObSeg = obstructedSegment;
            Gizmos.color = Color.red;
            while (pObSeg != null)
            {
                if (drawWithGizmos)
                    Gizmos.DrawLine(pointB + tangentBC * pObSeg.start, pointB + tangentBC * pObSeg.end);
                else
                    Debug.DrawLine(pointB + tangentBC * pObSeg.start, pointB + tangentBC * pObSeg.end, Color.red);
                pObSeg = pObSeg.next;
            }
        }

        public class ObstructedSegment
        {
            public float start;
            public float end;
            public ObstructedSegment next;

            public ObstructedSegment()
            {
                start = 0;
                end = 0;
            }

            public ObstructedSegment(SerializablePointNode.SerializableObstructedSegment src, ObstructedSegment next)
            {
                start = src.start;
                end = src.end;
                this.next = next;
            }

            public ObstructedSegment(float start, float end)
            {
                this.start = start;
                this.end = end;
            }
        }
    }

    /// <summary>
    /// A serializable version of the MarkableContour class.
    /// </summary>
    [Serializable]
    public class SerializableMarkableContour
    {
        public Bounds bounds;
        public bool isSolid;
        public bool isClosed;
        public SerializablePointNode[] points;

        public SerializableMarkableContour(MarkableContour src)
        {
            bounds = src.bounds;
            isSolid = src.isSolid;

            points = new SerializablePointNode[src.pointNodeCount];
            PointNode pn = src.firstPoint;
            for (int i = 0; i < src.pointNodeCount; i++, pn = pn.Next)
            {
                points[i] = new SerializablePointNode(pn);
            }
        }
    }

    /// <summary>
    /// A serializable version of the PointNode class.
    /// </summary>
    [Serializable]
    public class SerializablePointNode
    {
        public SerializableObstructedSegment[] obstructedSegments;
        public float angle; // in rads
        public float distanceBC;
        public Vector2 tangentBC;
        public bool isPointWalkable;
        public Vector2 pointB;

        public SerializablePointNode(PointNode src)
        {
            angle = src.angle;
            distanceBC = src.distanceBC;
            tangentBC = src.tangentBC;
            isPointWalkable = src.isPointWalkable;
            pointB = src.pointB;

            int obstructionCount = 0;
            PointNode.ObstructedSegment cSeg = src.FirstObstructedSegment;
            while (cSeg != null)
            {
                obstructionCount++;
                cSeg = cSeg.next;
            }

            obstructedSegments = new SerializableObstructedSegment[obstructionCount];
            obstructionCount = 0;
            cSeg = src.FirstObstructedSegment;
            while (cSeg != null)
            {
                obstructedSegments[obstructionCount] = new SerializableObstructedSegment(cSeg);
                obstructionCount++;
                cSeg = cSeg.next;
            }
        }

        /// <summary>
        /// A serializable version of the ObstructedSegment class.
        /// </summary>
        [Serializable]
        public class SerializableObstructedSegment
        {
            public float start;
            public float end;

            public SerializableObstructedSegment(PointNode.ObstructedSegment src)
            {
                this.start = src.start;
                this.end = src.end;
            }
        }
    }
}
