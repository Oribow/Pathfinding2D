using UnityEngine;
using System.Collections.Generic;
using Utility.ExtensionMethods;

namespace Utility.Polygon2D
{
    internal class PointChain : IVisualizable
    {
        public bool IsClosed { get; set; }
        public bool IsEmpty { get { return chain.Count == 0; } }

        public Vector2 FirstPoint
        {
            get { return chain.First.Value; }
        }
        public Vector2 LastPoint
        {
            get { return chain.Last.Value; }
        }
        public LinkedList<Vector2> chain;

        public PointChain(ref Vector2 p0, ref Vector2 p1, bool isClosed = false)
        {
            chain = new LinkedList<Vector2>();
            chain.AddLast(p0);
            chain.AddLast(p1);
            IsClosed = isClosed;
        }

        public PointChain(IEnumerable<Vector2> verts, bool isClosed = false)
        {
            chain = new LinkedList<Vector2>(verts);
            if (chain.Count < 2)
                throw new System.ArgumentOutOfRangeException("verts", "Verts must contain at least two items.");

            IsClosed = isClosed;
        }

        public bool LinkSegment(ref Vector2 p0, ref Vector2 p1)
        {
            /*if (p0 == FirstPoint)
            {
                if (p1 == LastPoint)
                    IsClosed = true;
                else
                {
                    chain.AddFirst(p1);

                    //Update bounds
                    bounds.min = Vector2.Min(bounds.min, p1);
                    bounds.max = Vector2.Max(bounds.max, p1);
                }
                return true;
            }
            if (p1 == LastPoint)
            {
                if (p0 == FirstPoint)
                    IsClosed = true;
                else
                {
                    chain.AddLast(p0);

                    //Update bounds
                    bounds.min = Vector2.Min(bounds.min, p0);
                    bounds.max = Vector2.Max(bounds.max, p0);
                }
                return true;
            }*/
            if (p1 == FirstPoint)
            {
                if (p0 == LastPoint)
                    IsClosed = true;
                else
                {
                    chain.AddFirst(p0);
                }
                return true;
            }
            if (p0 == LastPoint)
            {
                if (p1 == FirstPoint)
                    IsClosed = true;
                else
                {
                    chain.AddLast(p1);
                }
                return true;
            }
            return false;
        }

        public bool LinkPointChain(PointChain other)
        {
            if (other.FirstPoint == LastPoint)
            {
                other.chain.RemoveFirst();
                chain.AppendRange(other.chain);
            }
            else if (other.LastPoint == FirstPoint)
            {
                chain.RemoveFirst();
                chain.PrependRange(other.chain);
            }
            else if (other.FirstPoint == FirstPoint)
            {
                Debug.Log("Shouldn't happen (firstPoint == Firstpoint) and will lead to a wrapping issue!");
                chain.RemoveFirst();
                other.chain.Reverse();
                chain.PrependRange(other.chain);
            }
            else if (other.LastPoint == LastPoint)
            {
                Debug.Log("Shouldn't happen (LastPoint == LastPoint) and will lead to a wrapping issue!");
                chain.RemoveLast();
                other.chain.Reverse();
                chain.AppendRange(other.chain);
            }
            else
                return false; //Other PointChain couldnt be attached
            return true;
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

            LinkedListNode<Vector2> chainNode = chain.First;
            while ((chainNode = chainNode.Next) != null)
            {
                if (drawWithGizmos)
                {
                    Gizmos.DrawLine(chainNode.Previous.Value, chainNode.Value);
                    DebugExtension.DrawCircle(chainNode.Previous.Value, Vector3.forward, secondaryColor, indicatorSize);
                }
                else
                {
                    Debug.DrawLine(chainNode.Previous.Value, chainNode.Value, primaryColor);
                    DebugExtension.DebugCircle(chainNode.Previous.Value, Vector3.forward, secondaryColor, indicatorSize);
                }

            }
            if (IsClosed)
            {
                if (drawWithGizmos)
                {
                    Gizmos.DrawLine(chain.Last.Value, chain.First.Value);
                    DebugExtension.DrawCircle(chain.Last.Value, Vector3.forward, secondaryColor, indicatorSize);
                }
                else
                {
                    Debug.DrawLine(chain.Last.Value, chain.First.Value, primaryColor);
                    DebugExtension.DebugCircle(chain.Last.Value, Vector3.forward, secondaryColor, indicatorSize);
                }
            }
        }
    }
}
