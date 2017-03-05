using UnityEngine;
using UnityEditor;
using NavData2d.Editor;

namespace Pathfinding2d.NavDataGeneration
{
    public class NavData2dBuildWindow : EditorWindow, INavDataBuilder
    {

        [MenuItem("Pathfinding/NavData2DBuilder")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(NavData2dBuildWindow));
        }

        [SerializeField]
        NavData2dBuildContainer buildContainer;
        [SerializeField]
        TabManger tabManager;
        Vector2 scrollPos;

        public NavData2dBuildContainer GlobalBuildContainer
        {
            get
            {
                return buildContainer;
            }

            set
            {
                buildContainer = value;
            }
        }

        public void TriggerRepaint()
        {
            Repaint();
        }

        void OnEnable()
        {
            // Remove delegate listener if it has previously
            // been assigned.
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            // Add (or re-add) the delegate.
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;

            if (tabManager == null || tabManager.TabCount == 0)
                InitTabManager();
        }

        void OnDisable()
        {
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            tabManager.UnselectTab();
            buildContainer.UpdateAssetRef();
            EditorUtility.SetDirty(buildContainer);
            AssetDatabase.SaveAssets();
        }

        void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(false));
            tabManager.DoLayoutGUI();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);

            if (tabManager.CurrentTab != null)
                tabManager.CurrentTab.OnGUI();

           
        }

        void InitTabManager()
        {
            tabManager = new TabManger(new BuildContainerSelector(this),
                new NavAgentConfigurator(this),
                new ColliderSelector(this),
                new ContourTreeBuilder(this),
                new OutlineSelector(this),
                new NavDataGeometryBuilder(this),
                new NavNodeSelector(this),
                new JumpLinkPlacer(this),
                new FinalNavData2dPacker(this));
        }

        void OnSceneGUI(SceneView sceneView)
        {
            if (tabManager.CurrentTab != null)
                tabManager.DoSceneGUI(sceneView);
        }
    }
}
