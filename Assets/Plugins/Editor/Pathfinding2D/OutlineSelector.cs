using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Utility;

namespace NavGraph.Build
{
    [System.Serializable]
    class OutlineSelector// : ITab
    {/*
        public bool IsEnabled
        {
            get
            {
                return navBuilder.GlobalBuildContainer != null && navBuilder.GlobalBuildContainer.contourTree != null;
            }
        }

        public string TabHeader
        {
            get
            {
                return "OutlineSelector";
            }
        }

        INavDataBuilder navBuilder;
        [SerializeField]
        ContourOptionHolder[] contourOptions;
        [SerializeField]
        Vector2 globalScrollPos;
        [SerializeField]
        int selectedContour;
        [SerializeField]
        Vector3[][] treeVerts;
        Rect startElement;

        bool mouseClicked;

        public OutlineSelector(INavDataBuilder navBuilder)
        {
            this.navBuilder = navBuilder;
        }

        public void OnGUI()
        {
            if (Event.current.type == EventType.Repaint)
            {
                startElement = GUILayoutUtility.GetLastRect();
                startElement.y = 0;
            }
            mouseClicked = Event.current.type == EventType.MouseUp && Event.current.button == 0;
            HandleEvents();

            int prevIndentLevel = EditorGUI.indentLevel;
            int contourIndex = 0;
            bool childrenCanIgnoreUse = true;

            globalScrollPos = EditorGUILayout.BeginScrollView(globalScrollPos);

            DoContourLayout(navBuilder.GlobalBuildContainer.contourTree.FirstNode.children, ref contourIndex, childrenCanIgnoreUse);

            EditorGUILayout.EndScrollView();

            EditorGUI.indentLevel = prevIndentLevel;

            if (mouseClicked)
            {
                selectedContour = -1;
                mouseClicked = false;
                SceneView.RepaintAll();
                navBuilder.TriggerRepaint();
            }
        }

        void DoContourLayout(List<ContourNode> src, ref int contourIndex, bool childrenCanIgnoreUse)
        {
            for (int iNode = 0; iNode < src.Count; iNode++)
            {
                if (contourOptions[contourIndex].use)
                {
                    childrenCanIgnoreUse = false;
                    break;
                }
            }
            int indentLevelCache = EditorGUI.indentLevel + 1;
            bool guiEnabledCache = GUI.enabled;
            for (int iNode = 0; iNode < src.Count; iNode++)
            {
                GUI.enabled = guiEnabledCache;
                EditorGUI.indentLevel = indentLevelCache;
                var contour = src[iNode];

                if (mouseClicked && selectedContour != contourIndex)
                {
                    Rect box = startElement;
                    box.y += box.height * contourIndex;
                    if (box.Contains(Event.current.mousePosition))
                    {
                        selectedContour = contourIndex;
                        mouseClicked = false;
                        SceneView.RepaintAll();
                        navBuilder.TriggerRepaint();
                    }
                }
                if (selectedContour == contourIndex)
                {
                    Rect box = startElement;
                    box.y += box.height * contourIndex;
                    EditorGUI.DrawRect(box, Color.blue);
                }

                EditorGUILayout.BeginHorizontal();
                if (contour.children.Count == 0)
                    EditorGUILayout.LabelField(contourIndex+"C: " + contour.contour.VertexCount + " Verts");
                else
                    contourOptions[contourIndex].foldout = EditorGUILayout.Foldout(contourOptions[contourIndex].foldout, contourIndex + "C: " + contour.contour.VertexCount + " Verts");
                EditorGUI.BeginChangeCheck();
                contourOptions[contourIndex].use = EditorGUILayout.Toggle("Use", contourOptions[contourIndex].use);
                if (EditorGUI.EndChangeCheck())
                {
                    treeVerts = navBuilder.GlobalBuildContainer.contourTree.ToVertexArray();
                    navBuilder.GlobalBuildContainer.strippedContourTree = StripContourTree(navBuilder.GlobalBuildContainer.contourTree);
                }
                EditorGUILayout.EndHorizontal();

                if (contourOptions[contourIndex].foldout)
                {
                    GUI.enabled = childrenCanIgnoreUse || (GUI.enabled && contourOptions[contourIndex].use);
                    contourIndex++;
                    DoContourLayout(contour.children, ref contourIndex, childrenCanIgnoreUse);
                }
                else
                {
                    contourIndex += CountContourChildren(contour) + 1;
                }
            }
        }

        public void OnSceneGUI(SceneView sceneView)
        {
            if (selectedContour == -1)
            {
                int contourIndex = 0;
                DrawContour(navBuilder.GlobalBuildContainer.contourTree.FirstNode.children, ref contourIndex, true);
            }
            else
            {
                Vector3[] poly = treeVerts[selectedContour];
                Handles.color = Color.red;
                Handles.DrawAAPolyLine(4f, poly);
                Handles.DrawAAPolyLine(4f, poly[0], poly[poly.Length - 1]);
            }
        }

        public void OnSelected()
        {
            CreateContourOptionTree();
        }

        public void OnUnselected()
        {
            
        }

        void CreateContourOptionTree()
        {
            int contourCount = 0;

            foreach (var c in navBuilder.GlobalBuildContainer.contourTree)
                contourCount++;

            if (contourOptions == null || contourCount != contourOptions.Length)
            {
                contourOptions = new ContourOptionHolder[contourCount];
                for (int iContour = 0; iContour < contourCount; iContour++)
                {
                    contourOptions[iContour].foldout = true;
                    contourOptions[iContour].use = true;
                }
            }
            if (treeVerts == null || treeVerts.Length != contourCount)
                treeVerts = navBuilder.GlobalBuildContainer.contourTree.ToVertexArray();
        }

        void DrawContour(List<ContourNode> src, ref int contourIndex, bool childrenCanIgnoreUse)
        {
            for (int iNode = 0; iNode < src.Count; iNode++)
            {
                if (contourOptions[contourIndex].use)
                {
                    childrenCanIgnoreUse = false;
                    break;
                }
            }
            for (int iNode = 0; iNode < src.Count; iNode++)
            {
                if (contourOptions[contourIndex].use)
                {
                    DrawContour(contourIndex);
                    contourIndex++;
                    DrawContour(src[iNode].children, ref contourIndex, childrenCanIgnoreUse);
                }
                else if (childrenCanIgnoreUse)
                {
                    contourIndex++;
                    DrawContour(src[iNode].children, ref contourIndex, childrenCanIgnoreUse);
                }
                else
                {
                    contourIndex += CountContourChildren(src[iNode]) + 1;
                }
            }
        }

        void DrawContour(int contourIndex)
        {
            Vector3[] poly = treeVerts[contourIndex];
            Handles.color = DifferentColors.GetColor(contourIndex);
            Handles.DrawAAPolyLine(4f, poly);
            Handles.DrawAAPolyLine(4f, poly[0], poly[poly.Length - 1]);
        }

        void HandleEvents()
        {
            Event e = Event.current;
            if (e.type == EventType.keyDown && -1 != selectedContour)
            {
                if (e.keyCode == KeyCode.RightArrow)
                {
                    MoveIndex(1);
                    SceneView.RepaintAll();
                    navBuilder.TriggerRepaint();
                }
                else if (e.keyCode == KeyCode.LeftArrow)
                {
                    MoveIndex(-1);
                    SceneView.RepaintAll();
                    navBuilder.TriggerRepaint();
                }
            }
        }

        void MoveIndex(int dir)
        {
            int counter = 0;
            int newIndex = selectedContour;
            do
            {
                newIndex += dir;
                if (newIndex < 0)
                    newIndex = contourOptions.Length - 1;
                else if (newIndex >= contourOptions.Length)
                    newIndex = 0;
                counter++;
            } while (counter < contourOptions.Length && !contourOptions[newIndex].use);

            if (contourOptions[newIndex].use)
                selectedContour = newIndex;
        }

        ContourTree StripContourTree(ContourTree src)
        {
            ContourTree result = new ContourTree();
            int nodeIndex = 0;
            result.FirstNode.children = StripContours(OribowsUtilitys.DeepCopy<ContourNode[]>(src.FirstNode.children.ToArray(),
                new OribowsUtilitys.SerializationSurrogateContainer(new BoundsSerializationSurrogate(), typeof(Bounds), new System.Runtime.Serialization.StreamingContext(System.Runtime.Serialization.StreamingContextStates.All)),
                new OribowsUtilitys.SerializationSurrogateContainer(new Vector2SerializationSurrogate(), typeof(Vector2), new System.Runtime.Serialization.StreamingContext(System.Runtime.Serialization.StreamingContextStates.All))),
                ref nodeIndex, true);
            return result;
        }

        List<ContourNode> StripContours(ContourNode[] src, ref int nodeIndex, bool childrenCanIgnoreUse)
        {
            List<ContourNode> selectedChildren = new List<ContourNode>(src.Length);
            for (int iNode = 0; iNode < src.Length; iNode++)
            {
                if (contourOptions[nodeIndex].use)
                {
                    childrenCanIgnoreUse = false;
                    break;
                }
            }
            for (int iNode = 0; iNode < src.Length; iNode++)
            {
                if (contourOptions[nodeIndex].use)
                {
                    nodeIndex++;
                    selectedChildren.Add(src[iNode]);
                    src[iNode].children = StripContours(src[iNode].children.ToArray(), ref nodeIndex, childrenCanIgnoreUse);
                }
                else if (childrenCanIgnoreUse)
                {
                    nodeIndex++;
                    selectedChildren.AddRange(StripContours(src[iNode].children.ToArray(), ref nodeIndex, childrenCanIgnoreUse));
                }
                else
                {
                    nodeIndex += CountContourChildren(src[iNode]) + 1;
                }

            }
            return selectedChildren;
        }

        int CountContourChildren(ContourNode node)
        {
            Stack<ContourNode> nodesToProcess = new Stack<ContourNode>(node.children);
            int contourIndex = 0;
            while (nodesToProcess.Count > 0)
            {
                ContourNode cn = nodesToProcess.Pop();
                contourIndex++;
                for (int iOutline = 0; iOutline < cn.children.Count; iOutline++)
                {
                    nodesToProcess.Push(cn.children[iOutline]);
                }
            }
            return contourIndex;
        }

        struct ContourOptionHolder
        {
            public bool foldout;
            public bool use;
        }*/
    }
}
