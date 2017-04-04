using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;

namespace NavGraph.Build
{
    /// <summary>
    /// Holds a tree of Contours. A contour which is a child of another Contour is considered to be fully contained by it's parent.
    /// Hence a Solid Contour is always the child of a NonSolid Contour and vice versa. The first Contour is an empty NonSolid one.
    /// </summary>
    [Serializable]
    public class ContourTree : IEnumerable<ContourNode>
    {
        [SerializeField]
        ContourNode headNode; // root

        public ContourNode FirstNode { get { return headNode; } }

        public ContourTree()
        {
            //create an empty head node
            headNode = new ContourNode(null, false);
        }

        public static ContourTree Build(GeometrySet geometrySet)
        {
            ContourTree result = new ContourTree();
            //One by one add the polygons and build up the tree
            for (int iCol = 0; iCol < geometrySet.Polygons.Count; iCol++)
            {
                result.AddContour(geometrySet.Polygons[iCol]);
            }
            return result;
        }

        public void AddContour(Vector2[] verts)
        {
            bool wasContourUsed = false;
            headNode.AddSolidContour(new Contour(verts), ref wasContourUsed);
        }

        public Vector3[][] ToVertexArray()
        {
            List<Vector3[]> contourVerts = new List<Vector3[]>(30);
            foreach (var cn in this)
            {
                contourVerts.Add((cn.contour.GetVertexArray().Select(p => (Vector3)(p))).ToArray());
            }
            return contourVerts.ToArray();
        }

        public int ContourCount()
        {
            int count = 0;
            foreach (var contour in this)
                count++;
            return count;
        }

        public int VertexCount()
        {
            int count = 0;
            foreach (var contour in this)
            {
                count += contour.contour.VertexCount;
            }
            return count;
        }

        public IEnumerator<ContourNode> GetEnumerator()
        {
            return new ContourNodeIEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ContourNodeIEnumerator(this);
        }

        /// <summary>
        /// A depth first tree enumerator.
        /// </summary>
        public class ContourNodeIEnumerator : IEnumerator<ContourNode>
        {
            public ContourNode Current
            {
                get
                {
                    ContourNode result = tree.FirstNode;
                    foreach (var i in nodePointer)
                    {
                        result = result.children[i];
                    }
                    return result;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            List<int> nodePointer;
            ContourTree tree;

            public ContourNodeIEnumerator(ContourTree tree)
            {
                this.tree = tree;
                nodePointer = new List<int>(4);
                nodePointer.Add(-1);
            }

            public void Dispose()
            {

            }

            public bool MoveNext()
            {
                if (nodePointer[0] == -1)
                {
                    if (tree.FirstNode.children.Count == 0)
                        return false;
                    nodePointer[0] = 0;
                    return true;
                }

                ContourNode result = tree.FirstNode;
                for (int iNode = 0; iNode < nodePointer.Count - 1; iNode++)
                {
                    result = result.children[nodePointer[iNode]];
                }

                if (result.children[nodePointer[nodePointer.Count - 1]].children.Count > 0) //Current node has children, do them next
                {
                    nodePointer.Add(0);
                    return true;
                }

                if (nodePointer[nodePointer.Count - 1] + 1 < result.children.Count) //There is another node at the parent level
                {
                    nodePointer[nodePointer.Count - 1]++; //increase the index on the parent level
                    return true;
                }

                nodePointer.RemoveAt(nodePointer.Count - 1); //remove current node, as it is done!
                if (nodePointer.Count == 0)
                    return false;
                //Get the next node on a higher level
                int parentIndex = nodePointer.Count - 2;
                do
                {
                    result = tree.FirstNode;
                    for (int iNode = 0; iNode < nodePointer.Count - 1; iNode++)
                    {
                        result = result.children[nodePointer[iNode]];
                    }

                    if (nodePointer[nodePointer.Count - 1] + 1 < result.children.Count) //There is another node at this level
                    {
                        nodePointer[nodePointer.Count - 1]++;
                        return true;
                    }
                    nodePointer.RemoveAt(nodePointer.Count - 1); //remove current node, as it is done!
                } while ((--parentIndex) >= 0);

                //finished enumerating
                return false;
            }

            public void Reset()
            {
                nodePointer = new List<int>(4);
                nodePointer.Add(-1);
            }
        }
    }
}
