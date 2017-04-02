using UnityEditor;

namespace NavGraph
{
    public class NavData2DVisualizerWindow : EditorWindow
    {

        [MenuItem("Pathfinding/NavData2DVisualizer")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(NavData2DVisualizerWindow));
        }

        NavigationData2D navData2d;

        void OnGUI()
        {
            EditorGUILayout.LabelField("NavData Visualizer", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            navData2d = (NavigationData2D)EditorGUILayout.ObjectField("NavData2d", navData2d, typeof(NavigationData2D), false);
            if (EditorGUI.EndChangeCheck())
                SceneView.RepaintAll();
        }

        void OnSceneGUI(SceneView sceneView)
        {
            if (navData2d != null)
                navData2d.SceneDrawNavData2D();
        }

        void OnEnable()
        {
            // Remove delegate listener if it has previously
            // been assigned.
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            // Add (or re-add) the delegate.
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
        }

        void OnDestroy()
        {
            // When the window is destroyed, remove the delegate
            // so that it will no longer do any drawing.
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            SceneView.RepaintAll();
        }
    }
}
