using UnityEngine;
using System.Collections;
using System;
using UnityEditor;
using System.Collections.Generic;
using NavMesh2D.Core;
using Utility;
using System.Linq;

namespace NavData2d.Editor
{
    [System.Serializable]
    public class ContourTreeBuilder : ITab
    {
        enum DebugOption { Outline, Unoptimized, None }
        public bool IsEnabled
        {
            get
            {
                return navBuilder.GlobalBuildContainer != null && navBuilder.GlobalBuildContainer.colliderSet != null;
            }
        }

        public string TabHeader
        {
            get
            {
                return "ContourBuilder";
            }
        }
        ContourTree optimizedTree { get { return navBuilder.GlobalBuildContainer.contourTree; } set { navBuilder.GlobalBuildContainer.contourTree = value; } }

        INavDataBuilder navBuilder;
        [SerializeField]
        bool autoUpdate;
        [SerializeField]
        ContourTree unoptimizedTree;
        [SerializeField]
        DebugOption debugOption;
        Vector3[][] unoptimizedTreeVerts;
        Vector3[][] optimizedTreeVerts;

        public ContourTreeBuilder(INavDataBuilder navBuilder)
        {
            this.navBuilder = navBuilder;
        }

        public void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            navBuilder.GlobalBuildContainer.nodeMergeDistance = EditorGUILayout.Slider("Merge Distance", navBuilder.GlobalBuildContainer.nodeMergeDistance, 0.001f, .5f);
            navBuilder.GlobalBuildContainer.maxEdgeDeviation = EditorGUILayout.Slider("Max Edge Deviation", navBuilder.GlobalBuildContainer.maxEdgeDeviation, 0.0f, 5f);
            bool paramChanged = EditorGUI.EndChangeCheck();

            autoUpdate = EditorGUILayout.Toggle("Auto Update", autoUpdate);

            EditorGUI.BeginChangeCheck();
            debugOption = (DebugOption)EditorGUILayout.EnumPopup("Debug", debugOption);
            if (EditorGUI.EndChangeCheck())
                SceneView.RepaintAll();

            if (paramChanged && autoUpdate)
            {
                if (unoptimizedTree == null)
                    RebuildContourTree();
                else
                    TweakContourTree();
            }
            if (GUILayout.Button("Update"))
            {
                if (unoptimizedTree == null)
                    RebuildContourTree();
                else
                    TweakContourTree();
            }
            if (GUILayout.Button("Rebuild"))
            {
                RebuildContourTree();
            }
        }

        public void OnSceneGUI(SceneView sceneView)
        {
            if (debugOption == DebugOption.Outline)
            {
                if (optimizedTree != null)
                {
                    DrawPolygonArray(optimizedTreeVerts);
                }
            }
            else if (debugOption == DebugOption.Unoptimized)
            {
                if (unoptimizedTree != null)
                {
                    DrawPolygonArray(unoptimizedTreeVerts);
                }
            }
        }

        public void OnSelected()
        {
            if(optimizedTree != null)
                optimizedTreeVerts = CollectOutlineVerts(optimizedTree);
            if(unoptimizedTree != null)
                unoptimizedTreeVerts = CollectOutlineVerts(unoptimizedTree);
        }

        public void OnUnselected()
        {
            
        }

        void DrawPolygonArray(Vector3[][] polygons)
        {
            for (int iPoly = 0; iPoly < polygons.Length; iPoly++)
            {
                Vector3[] poly = polygons[iPoly];
                Handles.color = Color.white;
                foreach (var vert in poly)
                {
                    Handles.DrawWireDisc(vert, Vector3.forward, 0.1f);
                }
                Handles.color = DifferentColors.GetColor(iPoly);
                Handles.DrawAAPolyLine(4f, poly);
                Handles.DrawAAPolyLine(4f, poly[0], poly[poly.Length - 1]);
            }
        }

        Vector3[][] CollectOutlineVerts(ContourTree tree)
        {
            List<Vector3[]> contourVerts = new List<Vector3[]>(30);
            Stack<ContourNode> nodesToProcess = new Stack<ContourNode>(tree.FirstNode.children);

            while (nodesToProcess.Count > 0)
            {
                ContourNode cn = nodesToProcess.Pop();
                contourVerts.Add((cn.contour.verticies.Select(p => (Vector3)((Vector2)p))).ToArray());
                for (int iOutline = 0; iOutline < cn.children.Count; iOutline++)
                {
                    nodesToProcess.Push(cn.children[iOutline]);
                }
            }
            return contourVerts.ToArray();
        }

        void RebuildContourTree()
        {
            var collisionGeometrySet = navBuilder.GlobalBuildContainer.colliderSet.ToCollisionGeometrySet();
            unoptimizedTree = ContourTree.Build(collisionGeometrySet);
            unoptimizedTreeVerts = CollectOutlineVerts(unoptimizedTree);
            TweakContourTree();
        }

        void TweakContourTree()
        {
            optimizedTree = OribowsUtilitys.DeepCopy<ContourTree>(unoptimizedTree);
            optimizedTree.Optimize(navBuilder.GlobalBuildContainer.nodeMergeDistance, navBuilder.GlobalBuildContainer.maxEdgeDeviation);
            optimizedTreeVerts = CollectOutlineVerts(optimizedTree);
            if (optimizedTree != null && navBuilder.GlobalBuildContainer.strippedContourTree == null)
            {
                navBuilder.GlobalBuildContainer.strippedContourTree = optimizedTree;
            }
            SceneView.RepaintAll();
        }
    }
}
