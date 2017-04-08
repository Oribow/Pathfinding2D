using EditorUI;
using System.Collections;
using System.Collections.Generic;

namespace NavGraph.Build
{
    [System.Serializable]
    public class StripContourNodeHolder : IHierarchyElement, IEnumerable<StripContourNodeHolder>
    {

            public IEnumerable<IHierarchyElement> Children
            {
                get
                {
                    return children;
                }
            }

            public int ChildrenCount
            {
                get
                {
                    return children.Length;
                }
            }

            public object Data
            {
                get
                {
                    return this;
                }
            }

            public float HeightOffsetRelativeToParent
            {
                get; set;
            }

            public bool strip;
            public StripContourNodeHolder[] children;
            public ContourNode contourNode;


            public StripContourNodeHolder(ContourNode data)
            {
                this.contourNode = data;
            }

            public IHierarchyElement GetChildrenAt(int index)
            {
                return children[index];
            }

            public IEnumerator<StripContourNodeHolder> GetEnumerator()
            {
                return new ContourNodeHolderEnumerator(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public class ContourNodeHolderEnumerator : IEnumerator<StripContourNodeHolder>
            {
                public StripContourNodeHolder Current
                {
                    get
                    {
                    StripContourNodeHolder result = root;
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

                public List<int> nodePointer;
            StripContourNodeHolder root;
                bool ignoreStripOnce;

                public ContourNodeHolderEnumerator(StripContourNodeHolder root)
                {
                    this.root = root;
                    Reset();
                }

                public void Dispose()
                {
                    nodePointer = null;
                    root = null;
                }

                public bool MoveNext()
                {
                    while (DoMoveNext())
                    {
                        if (!Current.strip)
                            return true;
                    }
                    return false;
                }

                bool DoMoveNext()
                {
                    if (nodePointer.Count == 0)
                    {
                        if (root.children.Length == 0)
                            return false;
                        nodePointer.Add(0);
                        return true;
                    }

                    var currentNode = Current;
                    if (!currentNode.strip)
                        ignoreStripOnce = false;

                    if (currentNode.children.Length > 0) //Current node has children, do them next
                    {
                        if (!ignoreStripOnce)
                        {
                            for (int iChild = 0; iChild < currentNode.children.Length; iChild++)
                            {
                                if (!currentNode.children[iChild].strip)
                                {
                                    nodePointer.Add(iChild);
                                    return true;
                                }
                            }
                            //No child found, which wouldn't be striped.
                        }
                        else
                        {
                            nodePointer.Add(0);
                            return true;
                        }
                    }

                    //Get the next node on a higher level
                    do
                    {
                        var parentOfCurrentHolder = root;
                        for (int iNode = 0; iNode < nodePointer.Count - 1; iNode++)
                        {
                            parentOfCurrentHolder = parentOfCurrentHolder.children[nodePointer[iNode]];
                        }

                        if (nodePointer[nodePointer.Count - 1] + 1 < parentOfCurrentHolder.children.Length) //There is another node at this level
                        {
                            if (ignoreStripOnce)
                            {
                                nodePointer[nodePointer.Count - 1]++;
                                return true;
                            }
                            else
                            {
                                nodePointer[nodePointer.Count - 1]++;
                                while (nodePointer[nodePointer.Count - 1] < parentOfCurrentHolder.children.Length)
                                {
                                    if (!parentOfCurrentHolder.children[nodePointer[nodePointer.Count - 1]].strip)
                                        return true;
                                    nodePointer[nodePointer.Count - 1]++;
                                }
                            }
                        }
                        nodePointer.RemoveAt(nodePointer.Count - 1); //remove current node, as it is done!
                    } while (nodePointer.Count > 0);

                    //finished enumerating
                    return false;
                }

                public void Reset()
                {
                    nodePointer = new List<int>(4);
                    ignoreStripOnce = true;
                }
            }
    }
}
