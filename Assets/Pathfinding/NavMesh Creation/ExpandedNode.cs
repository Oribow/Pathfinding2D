using UnityEngine;
using System.Collections.Generic;
using System;

namespace Pathfinding2d.NavDataGeneration
{
    /// <summary>
    /// Holds a MarkableContour and a reference to MarkableContours contained by this Contour.
    /// </summary>
    [Serializable]
    internal class ExpandedNode : ISerializationCallbackReceiver
    {
        public List<ExpandedNode> children;

        [NonSerialized]
        public MarkableContour contour;
        [SerializeField]
        SerializableMarkableContour serializableMarkableContour;
       

        public ExpandedNode(ContourNode contourNode)
        {
            contour = new MarkableContour(contourNode.contour, contourNode.IsSolid);

            children = new List<ExpandedNode>(contourNode.children.Count);
            foreach (ContourNode cNode in contourNode.children)
                children.Add(new ExpandedNode(cNode));
        }

        public ExpandedNode()
        {
            children = new List<ExpandedNode>();
        }

        public void MarkContour(float minWalkableHeight)
        {
            if (contour == null)
            {
                for (int iChild = 0; iChild < children.Count; iChild++)
                {
                    WalkSpaceTester.MarkNotWalkableSegments(children[iChild].contour, children, minWalkableHeight);
                }
            }
            else if (!contour.isSolid)
            {
                if (children.Count == 0)
                    WalkSpaceTester.MarkSelfIntersections(contour, minWalkableHeight);
                else {
                    //Marks the children and this contour with all other children and this contour
                    //Self marking is prevented by the marking algorithm.
                    List<ExpandedNode> includingThis = new List<ExpandedNode>(children.Count + 1);
                    includingThis.AddRange(children);
                    includingThis.Add(this);
                    for (int iChild = 0; iChild < includingThis.Count; iChild++)
                    {
                        WalkSpaceTester.MarkNotWalkableSegments(children[iChild].contour, includingThis, minWalkableHeight);
                    }
                }
            }
            foreach (ExpandedNode eN in children)
                eN.MarkContour(minWalkableHeight);
        }

        public void OnBeforeSerialize()
        {
            serializableMarkableContour = new SerializableMarkableContour(contour);
        }

        public void OnAfterDeserialize()
        {
            contour = new MarkableContour(serializableMarkableContour);
            serializableMarkableContour = null;
        }
    }
}
