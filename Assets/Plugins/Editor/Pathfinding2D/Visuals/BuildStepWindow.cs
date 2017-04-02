using UnityEngine;
using System.Collections;
using UnityEditor;

namespace NavGraph.Build
{
    public abstract class BuildStepWindow : EditorWindow
    {
        
        protected NavGraphBuilderWindow BuildWin { get { return NavGraphBuilderWindow.Instance; } }
        bool isInitialized;


        protected abstract void InitThisWindow();
        protected abstract void DrawCustomGUI();
        protected abstract void DrawCustomSceneGUI(SceneView sceneView);

        protected virtual void OnFocus()
        {
            // Remove delegate listener if it has previously
            // been assigned.
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            // Add (or re-add) the delegate.
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
            SceneView.RepaintAll();
        }

        void OnGUI()
        {
            if (BuildWin == null)
            {
                EditorGUILayout.HelpBox("Couldn't find the main builder window.", MessageType.Error);
                isInitialized = false;
                return;
            }
            else if (BuildWin.BuildContainer == null)
            {
                EditorGUILayout.HelpBox("No build container selected.", MessageType.Error);
                isInitialized = false;
                return;
            }
            if (!isInitialized)
            {
                InitThisWindow();
                isInitialized = true;
            }
            DrawCustomGUI();
        }


        protected virtual void OnSceneGUI(SceneView sceneView)
        {
            if (EditorWindow.focusedWindow != this && EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.GetType() != typeof(SceneView) &&
                EditorWindow.focusedWindow.GetType() != typeof(NavGraphBuilderWindow))
                SceneView.onSceneGUIDelegate -= this.OnSceneGUI;

            if (BuildWin == null || BuildWin.BuildContainer == null)
                return;

            if (!isInitialized)
                return;

            DrawCustomSceneGUI(sceneView);
        }

        void OnDisable()
        {
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            SceneView.RepaintAll();
        }
    }
}
