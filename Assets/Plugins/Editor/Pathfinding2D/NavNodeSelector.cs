using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace NavGraph.Build
{
    [System.Serializable]
    class NavNodeSelector// : ITab
    {/*
        public bool IsEnabled
        {
            get
            {
                return navBuilder.GlobalBuildContainer != null && navBuilder.GlobalBuildContainer.prebuildNavData != null;
            }
        }

        public string TabHeader
        {
            get
            {
                return "NodeSelector";
            }
        }

        INavDataBuilder navBuilder;
        [SerializeField]
        ReorderableList nodeContainer;
        bool[] includeNode;
        Vector2 listScrollPos;

        public NavNodeSelector(INavDataBuilder navBuilder)
        {
            this.navBuilder = navBuilder;
        }

        public void OnGUI()
        {
            if (HandleEvents())
                SceneView.RepaintAll();
            listScrollPos = EditorGUILayout.BeginScrollView(listScrollPos);
            nodeContainer.DoLayoutList();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Manual Update"))
            {
                FilterNavNodes();
            }
            if (GUILayout.Button("Reset Changes"))
            {
                includeNode = Enumerable.Repeat(true, navBuilder.GlobalBuildContainer.prebuildNavData.nodes.Length).ToArray();
            }
            EditorGUILayout.EndHorizontal();
        }

        public void OnSceneGUI(SceneView sceneView)
        {
            if (HandleEvents())
            {
                navBuilder.TriggerRepaint();
            }
            for (int iNode = 0; iNode < navBuilder.GlobalBuildContainer.prebuildNavData.nodes.Length; iNode++)
            {
                NavNode nn = navBuilder.GlobalBuildContainer.prebuildNavData.nodes[iNode];

                if (iNode == nodeContainer.index)
                {
                    Handles.color = new Color(1, 1, 1);
                    Bounds bounds = new Bounds(nn.verts[0].PointB, Vector3.one);
                    for (int iVert = 1; iVert < nn.verts.Length; iVert++)
                    {
                        bounds.Encapsulate(nn.verts[iVert].PointB);
                    }
                    Handles.DrawWireCube(bounds.center, bounds.size);
                }
                else if (includeNode[iNode])
                {
                    Handles.color = Utility.DifferentColors.GetColor(iNode);
                }
                else
                    continue;
                for (int iVert = 0; iVert < nn.verts.Length - 1; iVert++)
                {
                    Handles.DrawLine(nn.verts[iVert].PointB, nn.verts[iVert + 1].PointB);
                    Handles.DrawWireDisc(nn.verts[iVert].PointB, Vector3.forward, 0.1f);
                }
                Handles.DrawWireDisc(nn.verts[nn.verts.Length - 1].PointB, Vector3.forward, 0.1f);
                if (nn.isClosed)
                {
                    Handles.DrawLine(nn.verts[nn.verts.Length - 1].PointB, nn.verts[0].PointB);

                }
            }
        }

        public void OnSelected()
        {
            if (includeNode == null || includeNode.Length != navBuilder.GlobalBuildContainer.prebuildNavData.nodes.Length)
            {
                includeNode = Enumerable.Repeat(true, navBuilder.GlobalBuildContainer.prebuildNavData.nodes.Length).ToArray();
            }
            if (nodeContainer == null)
                InitNodeContainer();
        }

        public void OnUnselected()
        {

        }

        bool HandleEvents()
        {
            Event e = Event.current;
            if (e.type == EventType.keyDown && -1 != nodeContainer.index)
            {
                if (e.keyCode == KeyCode.T)
                {
                    includeNode[nodeContainer.index] = !includeNode[nodeContainer.index];
                    MoveIndex(1);
                    return true;
                }
                else if (e.keyCode == KeyCode.RightArrow)
                {
                    MoveIndex(1);
                    return true;
                }
                else if (e.keyCode == KeyCode.LeftArrow)
                {
                    MoveIndex(-1);
                    return true;
                }
            }
            return false;
        }

        void MoveIndex(int dir)
        {
            int counter = 0;
            int newIndex = nodeContainer.index;
            do
            {
                newIndex += dir;
                if (newIndex < 0)
                    newIndex = includeNode.Length - 1;
                else if (newIndex >= includeNode.Length)
                    newIndex = 0;
                counter++;
            } while (counter < includeNode.Length && !includeNode[newIndex]);

            if (includeNode[newIndex])
                nodeContainer.index = newIndex;
        }

        void InitNodeContainer()
        {
            nodeContainer = new ReorderableList(navBuilder.GlobalBuildContainer.prebuildNavData.nodes, typeof(NavNode), false, true, false, false);
            nodeContainer.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                float completeWidth = rect.width;
                rect.height -= EditorGUIUtility.standardVerticalSpacing;
                rect.y += EditorGUIUtility.standardVerticalSpacing;
                rect.x += Mathf.Min(rect.width * 0.1f, 5);
                rect.width = Mathf.Min(rect.width * 0.4f, rect.height);
                GUI.enabled = !includeNode[index];
                if (GUI.Button(rect, "+"))
                {
                    includeNode[index] = true;
                    FilterNavNodes();
                }
                GUI.enabled = includeNode[index];
                rect.x += Mathf.Min(rect.width * 0.1f, 5) + rect.width;
                if (GUI.Button(rect, "-"))
                {
                    includeNode[index] = false;
                    FilterNavNodes();
                }
                GUI.enabled = true;
            };
            nodeContainer.drawElementBackgroundCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                if (includeNode[index])
                {
                    float newWidth = Mathf.Min(60, (rect.width * 0.6f) / 2);
                    float newHeight = Mathf.Min(5, (rect.height * 0.6f) / 2);
                    Rect smallerRect = new Rect(rect.x + newWidth, rect.y + newHeight, rect.width - newWidth * 2, rect.height - newHeight * 2);
                    EditorGUI.DrawRect(rect, isFocused ? Color.blue : Color.gray);
                    EditorGUI.DrawRect(smallerRect, Utility.DifferentColors.GetColor(index));
                }
                else
                    EditorGUI.DrawRect(rect, isFocused ? Color.blue : Color.gray);
            };
            nodeContainer.elementHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;
            nodeContainer.onSelectCallback = (ReorderableList list) =>
            {
                SceneView.RepaintAll();
            };
            nodeContainer.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "NavNodes  (" + includeNode.Length + ")");
            };
        }

        void FilterNavNodes ()
        {
            List<NavNode> filteredNodes = new List<NavNode>(navBuilder.GlobalBuildContainer.prebuildNavData.nodes.Length);

            for (int iNode = 0; iNode < navBuilder.GlobalBuildContainer.prebuildNavData.nodes.Length; iNode++)
            {
                if (includeNode[iNode])
                    filteredNodes.Add(navBuilder.GlobalBuildContainer.prebuildNavData.nodes[iNode]);
            }
            navBuilder.GlobalBuildContainer.filteredNavData = new NavigationData2D()
            {
                nodes = filteredNodes.ToArray(),
                name = navBuilder.GlobalBuildContainer.prebuildNavData.name,
                version = navBuilder.GlobalBuildContainer.prebuildNavData.version
            };
            EditorUtility.SetDirty(navBuilder.GlobalBuildContainer.filteredNavData);
        }*/
    }
}
