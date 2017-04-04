using UnityEngine;
using UnityEditor;
using Utility;

namespace NavGraph.Build
{
    [System.Serializable]
    public class ContourTreeBuilder : BuildStepWindow
    {
        enum DebugOption { Outline, Unoptimized, None }

        ContourTree optimizedTree { get { return BuildSave.OptimizedContourTree; } set { BuildSave.OptimizedContourTree = value; } }
        ContourTree unoptimizedTree { get { return BuildSave.VanilaContourTree; } set { BuildSave.VanilaContourTree = value; } }

        [SerializeField]
        DebugOption debugOption;


        protected override void InitThisWindow()
        {
            this.titleContent = new GUIContent("ContourTreeBuilder");
        }

        protected override void DrawCustomGUI()
        {
            EditorGUI.BeginChangeCheck();
            BuildSave.ContourTreeBuilderData.nodeMergeDistance = EditorGUILayout.Slider("Merge Distance", BuildSave.ContourTreeBuilderData.nodeMergeDistance, 0.001f, .5f);
            BuildSave.ContourTreeBuilderData.maxEdgeDeviation = EditorGUILayout.Slider("Max Edge Deviation", BuildSave.ContourTreeBuilderData.maxEdgeDeviation, 0.0f, 5f);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(BuildSave.ContourTreeBuilderData);
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
                if (BuildSave.OptimizedContourTreeVerts != null)
                {
                    DrawPolygonArray(BuildSave.OptimizedContourTreeVerts);
                }
            }
            else if (debugOption == DebugOption.Unoptimized)
            {
                if (BuildSave.VanilaContourTreeVerts != null)
                {
                    DrawPolygonArray(BuildSave.VanilaContourTreeVerts);
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
