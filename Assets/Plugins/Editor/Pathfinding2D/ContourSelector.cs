using EditorUI;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Utility;
using System.Collections;

namespace NavGraph.Build
{
    [System.Serializable]
    class ContourSelector : BuildStepWindow
    {
        [SerializeField]
        Vector2 globalScrollPos;
        [SerializeField]
        float boxPercentageOfWindow = 0.6f;
        [SerializeField]
        HierarchyList hierarchyList;
        [SerializeField]
        WindowOverlappingBox contourInspector;
        [SerializeField]
        Bounds optimizedContourTreeBounds;
        [SerializeField]
        float treeAspectRatio;
        [SerializeField]
        ContourNode selectedContour;
        [SerializeField]
        bool stripedTreeNeedsUpdate;


        protected override void InitThisWindow()
        {
            BuildSave.CreateContourNodeHolderTree();
            stripedTreeNeedsUpdate = true;

            hierarchyList = new HierarchyList(BuildSave.RootContourNodeHolder);
            hierarchyList.elementDimension = new Vector2(30, 30);
            hierarchyList.elementDrawCallback = DrawContourElement;

            //Chache bounds of tree
            optimizedContourTreeBounds = BuildSave.OptimizedContourTree.GetBounds();

            treeAspectRatio = optimizedContourTreeBounds.extents.y / optimizedContourTreeBounds.extents.x;
            contourInspector = new WindowOverlappingBox(WindowOverlappingBox.Placement.UpperRightCorner, false, new Vector2(boxPercentageOfWindow, boxPercentageOfWindow * treeAspectRatio), 3);
            contourInspector.borderColor = Color.gray;
            contourInspector.DrawContentCallback += DrawContourInspector;
            wantsMouseMove = true;
        }

        protected override void DrawCustomGUI()
        {
            if (Event.current.type == EventType.MouseMove)
            {
                Repaint();
                return;
            }
            globalScrollPos = EditorGUILayout.BeginScrollView(globalScrollPos);

            EditorGUILayout.BeginHorizontal();
            var oldSelectedContour = selectedContour;
            selectedContour = null;
            hierarchyList.DoLayout();

            float contextWidth = (float)typeof(EditorGUIUtility).GetProperty("contextWidth", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null, null);

            GUILayout.Space(contextWidth * boxPercentageOfWindow);

            EditorGUILayout.EndHorizontal();

            
            EditorGUILayout.EndScrollView();

            Rect realContentPane = position;
            realContentPane.width = contextWidth;
            contourInspector.boxDimensions.y = (treeAspectRatio * contextWidth * boxPercentageOfWindow) / (position.height) + (10 / position.height);
            contourInspector.DoLayout(realContentPane);

            if (oldSelectedContour != selectedContour)
                SceneView.RepaintAll();
        }

        protected override void DrawCustomSceneGUI(SceneView sceneView)
        {
            if (BuildSave.RootContourNodeHolder == null)
                return;

            foreach (var holder in BuildSave.RootContourNodeHolder)
            {
                if (holder.contourNode == selectedContour)
                    Handles.color = Color.red;
                else
                    Handles.color = Color.white;

                Vector3[] poly = holder.contourNode.contour.GetVertex3dArray();
                Handles.DrawAAPolyLine(4, poly);
                Handles.DrawAAPolyLine(4f, poly[0], poly[poly.Length - 1]);
            }
        }

        void DrawContourElement(Rect rect, object data)
        {
            StripContourNodeHolder nodeHolder = (StripContourNodeHolder)data;
            ContourNode node = (ContourNode)nodeHolder.contourNode;
            if (node.contour.VertexCount == 0)
            {
                EditorGUI.DrawRect(rect, Color.gray);
                return;
            }
            if (rect.Contains(Event.current.mousePosition))
            {
                selectedContour = node;
                if (Event.current.button == 0 && Event.current.type == EventType.mouseDown)
                {
                    Event.current.Use();
                    nodeHolder.strip = !nodeHolder.strip;
                    stripedTreeNeedsUpdate = true;
                }
            }
            EditorGUI.DrawRect(rect, (nodeHolder.strip) ? Color.gray : Color.black);

            float margin = 2;
            rect.xMin += margin;
            rect.yMin += margin;
            rect.xMax -= margin;
            rect.yMax -= margin;


            float scaleFactor = (node.contour.Bounds.size.x > node.contour.Bounds.size.y) ? rect.width / node.contour.Bounds.size.x : rect.height / node.contour.Bounds.size.y;
            Vector3 scale = new Vector3(scaleFactor, scaleFactor, 1);
            Matrix4x4 scaleMat = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(180, 0, 0), scale);

            Vector3 translation = new Vector3(rect.x, rect.y, 0) - scaleMat.MultiplyPoint(new Vector3(node.contour.Bounds.min.x, node.contour.Bounds.max.y, 0));
            Matrix4x4 translationMat = Matrix4x4.identity;
            translationMat.SetColumn(3, new Vector4(translation.x, translation.y, translation.z, 1));
            Matrix4x4 transfromMat = translationMat * scaleMat;

            Handles.matrix = transfromMat;
            Handles.color = (nodeHolder.strip) ? Color.white : Color.white;
            Vector3 prevPoint = node.contour[node.contour.VertexCount - 1];
            foreach (Vector3 point in node.contour)
            {
                Handles.DrawLine(prevPoint, point);
                prevPoint = point;
            }
        }

        void DrawContourInspector(Rect rect)
        {
            float margin = 5;
            Rect marginRect = rect;
            marginRect.xMin += margin;
            marginRect.yMin += margin;
            marginRect.xMax -= margin;
            marginRect.yMax -= margin;
            Vector3 scale = new Vector3(marginRect.width / optimizedContourTreeBounds.size.x, marginRect.width / optimizedContourTreeBounds.size.x, 1);
            Matrix4x4 scaleMat = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(180, 0, 0), scale);

            Vector3 translation = new Vector3(marginRect.x, marginRect.y, 0) - scaleMat.MultiplyPoint(new Vector3(optimizedContourTreeBounds.min.x, optimizedContourTreeBounds.max.y, 0));
            Matrix4x4 translationMat = Matrix4x4.identity;
            translationMat.SetColumn(3, new Vector4(translation.x, translation.y, translation.z, 1));
            Matrix4x4 transfromMat = translationMat * scaleMat;

            Handles.matrix = transfromMat;

            foreach (var node in BuildSave.RootContourNodeHolder)
            {
                if (node.contourNode == selectedContour)
                {
                    Handles.color = Color.red;
                }
                else
                {
                    Handles.color = Color.black;
                }
                Vector3 prevPoint = node.contourNode.contour[node.contourNode.contour.VertexCount - 1];
                foreach (Vector3 point in node.contourNode.contour)
                {
                    Handles.DrawLine(prevPoint, point);
                    prevPoint = point;
                }
            }

            //Draw size buttons
            Rect buttonPos = rect;
            buttonPos.yMin = rect.yMax - EditorGUIUtility.singleLineHeight;
            buttonPos.xMin = rect.xMax - 42;
            buttonPos.width = 20;
            if (GUI.Button(buttonPos, "+"))
            {
                boxPercentageOfWindow = Mathf.Min(boxPercentageOfWindow + 0.05f, 0.9f);
                contourInspector.boxDimensions.x = boxPercentageOfWindow;
            }
            buttonPos.x = rect.xMax - 20;
            if (GUI.Button(buttonPos, "-"))
            {
                boxPercentageOfWindow = Mathf.Max(boxPercentageOfWindow - 0.05f, 0.1f);
                contourInspector.boxDimensions.x = boxPercentageOfWindow;
            }
        }

        
    }
}
