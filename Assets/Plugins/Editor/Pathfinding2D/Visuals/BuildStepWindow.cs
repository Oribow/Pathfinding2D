using UnityEngine;
using System.Collections;
using UnityEditor;

namespace NavGraph.Build
{
    public abstract class BuildStepWindow : EditorWindow
    {
        
        protected NavGraphBuilderWindow BuildWin { get { return NavGraphBuilderWindow.Instance; } }
        protected BuildProcessSave BuildSave { get { return BuildWin.BuildContainer; } }
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

        void OnEnable()
        {
            if (!isInitialized)
            {
                InitThisWindow();
                isInitialized = true;
            }
        }

        void OnGUI()
        {
            if (BuildWin == null)
            {
                EditorGUILayout.HelpBox("Couldn't find the main builder window.", MessageType.Error);
                isInitialized = false;
                return;
            }
            else if (BuildSave == null)
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
            try
            {
                DrawCustomGUI();
            }
            catch (System.Exception e)
            {
                this.Close();
                Debug.LogException(e);
            }
        }


        protected virtual void OnSceneGUI(SceneView sceneView)
        {
            if (EditorWindow.focusedWindow != this && EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.GetType() != typeof(SceneView) &&
                EditorWindow.focusedWindow.GetType() != typeof(NavGraphBuilderWindow))
                SceneView.onSceneGUIDelegate -= this.OnSceneGUI;

            if (BuildWin == null || BuildSave == null)
                return;

            if (!isInitialized)
                return;

            try
            {
                DrawCustomSceneGUI(sceneView);
            }
            catch (System.Exception e)
            {
                this.Close();
                Debug.LogException(e);
            }
        }

        void OnDisable()
        {
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            SceneView.RepaintAll();
        }
    }
}
