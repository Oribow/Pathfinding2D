using UnityEngine;
using UnityEditor;
using Utility;

namespace NavGraph.Build
{
    [System.Serializable]
    public class ContourTreeBuilder : BuildStepWindow
    {
        enum DebugOption { Outline, Unoptimized, None }

        ContourTree optimizedTree { get { return BuildWin.BuildContainer.ContourTree; } set { BuildWin.BuildContainer.ContourTree = value; } }
        ContourTree unoptimizedTree { get { return BuildWin.BuildContainer.unoptimizedTree; } set { BuildWin.BuildContainer.unoptimizedTree = value; } }

        [SerializeField]
        DebugOption debugOption;


        protected override void InitThisWindow()
        {
            if (optimizedTree != null)
                BuildWin.BuildContainer.optimizedTreeVerts = optimizedTree.ToVertexArray();
            if (unoptimizedTree != null)
                BuildWin.BuildContainer.unoptimizedTreeVerts = unoptimizedTree.ToVertexArray();
        }

        void OnEnable()
        {
            this.titleContent = new GUIContent("ContourTreeBuilder");
        }

        protected override void DrawCustomGUI()
        {
            EditorGUI.BeginChangeCheck();
            BuildWin.BuildContainer.ContourTreeBuilderData.nodeMergeDistance = EditorGUILayout.Slider("Merge Distance", BuildWin.BuildContainer.ContourTreeBuilderData.nodeMergeDistance, 0.001f, .5f);
            BuildWin.BuildContainer.ContourTreeBuilderData.maxEdgeDeviation = EditorGUILayout.Slider("Max Edge Deviation", BuildWin.BuildContainer.ContourTreeBuilderData.maxEdgeDeviation, 0.0f, 5f);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(BuildWin.BuildContainer.ContourTreeBuilderData);
                BuildWin.UpdateBuildStepInformation();
            }

            EditorGUI.BeginChangeCheck();
            debugOption = (DebugOption)EditorGUILayout.EnumPopup("Debug", debugOption);
            if (EditorGUI.EndChangeCheck())
                SceneView.RepaintAll();
        }

        protected override void DrawCustomSceneGUI(SceneView sceneView)
        {
            if (debugOption == DebugOption.Outline)
            {
                if (BuildWin.BuildContainer.optimizedTreeVerts != null)
                {
                    DrawPolygonArray(BuildWin.BuildContainer.optimizedTreeVerts);
                }
            }
            else if (debugOption == DebugOption.Unoptimized)
            {
                if (BuildWin.BuildContainer.unoptimizedTreeVerts != null)
                {
                    DrawPolygonArray(BuildWin.BuildContainer.unoptimizedTreeVerts);
                }
            }
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
    }
}
