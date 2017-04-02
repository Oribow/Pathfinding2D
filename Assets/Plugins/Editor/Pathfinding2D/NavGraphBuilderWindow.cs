using UnityEngine;
using UnityEditor;
using NavGraph.EditorUI;

namespace NavGraph.Build
{
    [System.Serializable]
    public class NavGraphBuilderWindow : EditorWindow
    {
        public static NavGraphBuilderWindow Instance { get { return instance; } }
        static NavGraphBuilderWindow instance;

        [MenuItem("NavGraph2D/Builder")]
        public static void ShowWindow()
        {
            instance = (NavGraphBuilderWindow)EditorWindow.GetWindow(typeof(NavGraphBuilderWindow));
        }


        public BuildProcessSave BuildContainer { get { return buildContainer; } }

        BuildStepUIList buildStepList;
        Vector2 scrollPos;
        Texture2D windowHeader;
        [SerializeField]
        BuildProcessSave buildContainer;

        public void UpdateBuildStepInformation()
        {
            if (buildContainer == null)
                return;

            var step0 = buildStepList.GetBuildStepAt(0);
            if (buildContainer.ColliderSet != null)
            {

                step0.ClearErrorsWarningsNotifications();
                if (buildContainer.ColliderSet.colliderList.Count == 0)
                {
                    step0.AddErrorNotification("You must at least select 1 collider.");
                    step0.buildStepStatus = BuildStepUIElement.BuildStepStatus.Notbuilt;
                }
                else
                {
                    step0.buildStepStatus = BuildStepUIElement.BuildStepStatus.OK;
                }
            }

            var step1 = buildStepList.GetBuildStepAt(1);
            step1.isEnabled = step0.buildStepStatus != BuildStepUIElement.BuildStepStatus.Notbuilt;
            if (buildContainer.ContourTree != null)
            {
                step1.ClearErrorsWarningsNotifications();
            }
            Repaint();
        }

        public void MarkFollowingStepsOutdated(int stepIndex)
        {
            for (int iStep = stepIndex + 1; iStep < buildStepList.BuildStepCount; iStep++)
            {
                buildStepList.GetBuildStepAt(iStep).buildStepStatus = BuildStepUIElement.BuildStepStatus.Outdated;
            }
        }

        void OnEnable()
        {
            this.titleContent = new GUIContent("NavGraphBuilder");

            if (instance == null)
                instance = this;

            if (windowHeader == null)
                windowHeader = EditorAssetLoadHelper.LoadAsset<Texture2D>(EditorAssetLoadHelper.EditorWindowHeader_Builder);

            if (buildStepList == null)
            {
                InitBuildStepManager();
            }
        }

        void OnDestroy()
        {
            //Close all opened Windows
            ContourTreeBuilder treeBuilder = EditorWindow.GetWindow<ContourTreeBuilder>();
            if (treeBuilder != null)
                treeBuilder.Close();

            ColliderSelector selector = EditorWindow.GetWindow<ColliderSelector>();
            if (selector != null)
                selector.Close();

            AssetDatabase.SaveAssets();
        }

        void OnGUI()
        {
            //Begin main scrollview
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            //Draw Header
            DrawHeader();

            //Save file selection
            DrawSaveFileSelection();

            //Draw buildSteps
            GUI.enabled = buildContainer != null;
            buildStepList.DrawLayout();
            GUI.enabled = true;

            //End main scrollview
            EditorGUILayout.EndScrollView();
        }

        void DrawHeader()
        {
            Rect headerRect = GUILayoutUtility.GetRect(GUIContent.none, CustomGUI.splitter, GUILayout.Height(EditorStyles.largeLabel.lineHeight));
            Rect backgroundRect = headerRect;
            backgroundRect.yMin = 0;
            EditorGUI.DrawRect(backgroundRect, Color.gray);
            GUI.Label(headerRect, "NavGraph Builder", EditorStyles.largeLabel);
        }

        void DrawSaveFileSelection()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Save File: " + ((buildContainer == null) ? "None" : buildContainer.assetName), EditorStyles.miniLabel);
            if (GUILayout.Button("Select", EditorStyles.miniButtonMid))
            {
                var newContainer = BuildProcessSave.SelectExistingAsset();
                if (newContainer != null)
                {
                    AssetDatabase.SaveAssets();
                    buildContainer = newContainer;
                    UpdateBuildStepInformation();
                }
            }
            if (GUILayout.Button("New", EditorStyles.miniButtonRight))
            {
                var newContainer = BuildProcessSave.SaveAsNewAsset();
                if (newContainer != null)
                {
                    AssetDatabase.SaveAssets();
                    buildContainer = newContainer;
                    UpdateBuildStepInformation();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        void InitBuildStepManager()
        {
            buildStepList = new BuildStepUIList();

            //ColliderSelector
            BuildStepUIElement uiElement = new BuildStepUIElement("Collider Selector", new string[] { "Configure" });
            uiElement.SetButtonDownListener((int buttonIndex) =>
            {
                if (buttonIndex == 0)
                {
                    //Launch the configuration window
                    EditorWindow.GetWindow<ColliderSelector>();
                }
            });
            uiElement.AddInformationalNotification(() => { return "Selected Collider: " + ((buildContainer == null || buildContainer.ColliderSet == null) ? "0" : buildContainer.ColliderSet.colliderList.Count.ToString()); });
            buildStepList.AddBuildStep(uiElement);

            //ContourBuilder
            uiElement = new BuildStepUIElement("Contour Builder", new string[] { "Build", "Apply Optimization", "Configure" });
            uiElement.SetButtonDownListener((int buttonIndex) =>
            {
                if (buttonIndex == 0)
                {
                    BuildContainer.RebuildContourTree();
                    MarkFollowingStepsOutdated(1);
                    buildStepList.GetBuildStepAt(1).buildStepStatus = BuildStepUIElement.BuildStepStatus.OK;
                }
                else if (buttonIndex == 1)
                {
                    if (BuildContainer.unoptimizedTree == null)
                        BuildContainer.RebuildContourTree();
                    else
                        BuildContainer.TweakContourTree();
                    MarkFollowingStepsOutdated(1);
                    buildStepList.GetBuildStepAt(1).buildStepStatus = BuildStepUIElement.BuildStepStatus.OK;
                }
                else if (buttonIndex == 2)
                {
                    //Launch the configuration window
                    EditorWindow.GetWindow<ContourTreeBuilder>();
                }
            });
            uiElement.AddInformationalNotification(() => { return "Contour count: " + ((buildContainer == null || buildContainer.ContourTree == null) ? "0" : buildContainer.ContourTree.ContourCount().ToString()); });
            uiElement.AddInformationalNotification(() => { return "Unoptimized Vertex count: " + ((buildContainer == null || buildContainer.unoptimizedTree == null) ? "0" : buildContainer.unoptimizedTree.VertexCount().ToString()); });
            uiElement.AddInformationalNotification(() => { return "Optimized Vertex count: " + ((buildContainer == null || buildContainer.ContourTree == null) ? "0" : buildContainer.ContourTree.VertexCount().ToString()); });
            buildStepList.AddBuildStep(uiElement);
        }
    }
}
