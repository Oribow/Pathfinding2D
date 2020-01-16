//#define DEBUG_JUMP_LINKS

using UnityEngine;
using System.Collections.Generic;

namespace NavGraph.Build
{
    /// <summary>
    /// Builds a NavData skeleton (no links) out of an ExpandedTree.
    /// </summary>
    public class NavigationData2DBuilder
    {
        const float fudgeFactor = 0.00001f;
        //NavAgentGroundWalkerSettings agentSettings;
        float minNodeLength;

        public NavigationData2DBuilder(/*NavAgentGroundWalkerSettings agentSettings,*/ float minNodeLength)
        {
            //this.agentSettings = agentSettings;
            this.minNodeLength = minNodeLength;
        }

        public void Build(ExpandedTree expandedTree, NavigationData2D dst)
        {
            List<NavNode> navNodes = new List<NavNode>(10); //-> make it not arbitrary!!
            foreach (ExpandedNode n in expandedTree.headNode.children)
                HandleMarkableContour(n, navNodes, 1);
            NavNode[] allNN = navNodes.ToArray();
            navNodes = null;
            dst.nodes = allNN;
        }

        private void HandleMarkableContour(ExpandedNode exNode, List<NavNode> nodes, int hierachyIndex)
        {
            ConvertMarkableContour(nodes, exNode.contour, hierachyIndex);
            foreach (ExpandedNode n in exNode.children)
                HandleMarkableContour(n, nodes, hierachyIndex + 1);
        }

        private void ConvertMarkableContour(List<NavNode> inOutNodes, MarkableContour mc, int hierachyIndex)
        {
            //Find start point
            PointNode startPointNode = mc.firstPoint;
            bool isClosed = true;

            foreach (PointNode pn in mc)
            {
                if (!pn.IsPointWalkable() || pn.FirstObstructedSegment == null && IsEdgeAcceptable(pn))
                {
                    startPointNode = pn;
                    isClosed = false;
                    break;
                }
            }
            DebugExtension.DebugCircle(startPointNode.pointB, Vector3.forward, Color.magenta, 0.5f);
            //startPointNode points now at the first obstruction
            List<NavVert> vertBuffer = new List<NavVert>(mc.pointNodeCount);
            Bounds inoutBounds = new Bounds(startPointNode.pointB, Vector3.zero);

            PointNode cPN = startPointNode;
            do
            {
                HandleEdge(cPN, inOutNodes, vertBuffer, ref inoutBounds, ref isClosed);
            } while ((cPN = cPN.Next) != startPointNode);


            HandleEdge(cPN, inOutNodes, vertBuffer, ref inoutBounds, ref isClosed);
            if (vertBuffer.Count > 1)
            {
                inOutNodes.Add(new NavNode(vertBuffer.ToArray(), inoutBounds, isClosed, hierachyIndex));
            }
        }

        private void HandleEdge(PointNode edge, List<NavNode> inoutNodes, List<NavVert> inoutVerts, ref Bounds inoutBounds, ref bool isClosed)
        {
            PointNode.ObstructedSegment obstrSeg = edge.FirstObstructedSegment;
            Vector2 startPoint = edge.pointB;

            if (!IsEdgeAcceptable(edge))
            {
                inoutVerts.Add(new NavVert(startPoint));
                inoutBounds.Encapsulate(startPoint);
                if (edge.Next != null)
                    EndNavNode(inoutNodes, inoutVerts, ref inoutBounds, edge.Next.pointB);
                else
                    EndNavNode(inoutNodes, inoutVerts, ref inoutBounds, Vector3.zero);
                isClosed = false;
                return;
            }

            if (obstrSeg == null)
            {
                inoutVerts.Add(new NavVert(startPoint));
                inoutBounds.Encapsulate(startPoint);
                return;
            }
            else if (obstrSeg.start == 0)
            {
                inoutVerts.Add(new NavVert(startPoint));
                inoutBounds.Encapsulate(startPoint);
                EndNavNode(inoutNodes, inoutVerts, ref inoutBounds, startPoint + edge.tangentBC * obstrSeg.end);
                if (obstrSeg.next == null)
                {
                    if (edge.distanceBC - obstrSeg.end > fudgeFactor)
                    {
                        startPoint = edge.pointB + edge.tangentBC * obstrSeg.end;
                        inoutVerts.Add(new NavVert(startPoint));
                        inoutBounds.Encapsulate(startPoint);
                    }
                    return;
                }
                obstrSeg = obstrSeg.next;
            }

            while (true)
            {
                inoutVerts.Add(new NavVert(startPoint));
                inoutBounds.Encapsulate(startPoint);
                inoutVerts.Add(new NavVert(edge.pointB + edge.tangentBC * obstrSeg.start));
                inoutBounds.Encapsulate(edge.pointB + edge.tangentBC * obstrSeg.start);
                startPoint = edge.pointB + edge.tangentBC * obstrSeg.end;
                EndNavNode(inoutNodes, inoutVerts, ref inoutBounds, startPoint);

                if (obstrSeg.next == null)
                    break;

                obstrSeg = obstrSeg.next;
            }

            if (edge.distanceBC - obstrSeg.end > fudgeFactor)
            {
                inoutVerts.Add(new NavVert(startPoint));
                inoutBounds.Encapsulate(startPoint);
            }
        }

        private void EndNavNode(List<NavNode> inoutNodes, List<NavVert> inoutVerts, ref Bounds inoutBounds, Vector2 nextPoint)
        {
            if (inoutVerts.Count > 1)
            {
                inoutNodes.Add(new NavNode(inoutVerts.ToArray(), inoutBounds, false, 0));
            }
            inoutBounds = new Bounds(nextPoint, Vector3.zero);
            inoutVerts.Clear();
        }

        private bool IsEdgeAcceptable(PointNode edge)
        {
            if (edge.distanceBC < minNodeLength)
                return false;

            if (true)
            {
                float angle = Vector2.Angle(Vector2.left, edge.tangentBC);
                /*if (Vector3.Cross(Vector2.left, edge.tangentBC).z < 0)
                    angle = 360 - angle;
                if (angle > 90)
                    angle = 180 - angle;*/
                /*if (angle > agentSettings.slopeLimit)
                {
                    return false;
                }
                else
                {
                    return true;
                }*/
            }
            else
            {
                /*
                if (pointNodeCount < 3 && !isClosed)
                    return;

                PointNode current = isClosed ? firstPoint : firstPoint.Next.Next;
                PointNode prevNode = current.Previous;
                PointNode prevPrevNode = prevNode.Previous;
                int edgeCount = isClosed ? pointNodeCount : pointNodeCount - 2;
                for (int iNode = 0; iNode < edgeCount; iNode++)
                {
                    if (prevNode.angle * Mathf.Rad2Deg > maxAngle)
                    {
                        //stage for remove
                    }

                    prevPrevNode = prevNode;
                    prevNode = current;
                    current = current.Next;
                }*/
            }
            return false;
        }
    }
}
