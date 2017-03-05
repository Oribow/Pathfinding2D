using NavMesh2D.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace NavData2d.Editor
{
    [System.Serializable]
    internal class JumpLinkPlacer : ITab
    {
        public bool IsEnabled
        {
            get
            {
                return navBuilder.GlobalBuildContainer != null && navBuilder.GlobalBuildContainer.filteredNavData;
            }
        }

        public string TabHeader
        {
            get
            {
                return "Jumping";
            }
        }

        List<MetaJumpLink> jumpLinks { get { return navBuilder.GlobalBuildContainer.jumpLinks; } set { navBuilder.GlobalBuildContainer.jumpLinks = value; } }

        INavDataBuilder navBuilder;

        public JumpLinkPlacer(INavDataBuilder navBuilder)
        {
            this.navBuilder = navBuilder;
        }

        public void OnSelected()
        {
            if (jumpLinks == null)
                jumpLinks = new List<MetaJumpLink>();
        }

        public void OnUnselected()
        {

        }


        Vector2 scrollPos;
        [SerializeField]
        bool showDetailsGlobal;
        [SerializeField]
        bool showInSceneGlobal;
        [SerializeField]
        bool showInSceneDetailedGlobal;

        public void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            showDetailsGlobal = EditorGUILayout.ToggleLeft("All Details", showDetailsGlobal, GUILayout.Width(75));
            if (EditorGUI.EndChangeCheck())
            {
                ToogleShowDetailAll(showDetailsGlobal);
            }

            EditorGUI.BeginChangeCheck();
            showInSceneGlobal = EditorGUILayout.ToggleLeft("All Visibility", showInSceneGlobal, GUILayout.Width(80));
            if (EditorGUI.EndChangeCheck())
            {
                ToogleShowInScene(showInSceneGlobal);
            }

            EditorGUI.BeginChangeCheck();
            showInSceneDetailedGlobal = EditorGUILayout.ToggleLeft("All Scene Details", showInSceneDetailedGlobal, GUILayout.Width(120));
            if (EditorGUI.EndChangeCheck())
            {
                ToogleShowInSceneDetailed(showInSceneDetailedGlobal);
            }

            if (GUILayout.Button("Select Invalid Links", GUILayout.Width(120)))
            {
                ShowInvalidLinks();
            }

            EditorGUILayout.EndHorizontal();

            MetaJumpLink link;
            for (int iLink = 0; iLink < jumpLinks.Count; iLink++)
            {
                EditorGUI.BeginChangeCheck();

                link = jumpLinks[iLink];
                EditorGUILayout.BeginHorizontal();
                link.jumpLinkSettings.showDetails = EditorGUILayout.Foldout(link.jumpLinkSettings.showDetails, "Link");
                link.isBiDirectional = EditorGUILayout.ToggleLeft(link.isBiDirectional ? "<->" : "->", link.isBiDirectional, GUILayout.Width(40));
                link.jumpLinkSettings.showInScene = EditorGUILayout.ToggleLeft(link.jumpLinkSettings.showInScene ? "[o]" : "[-]", link.jumpLinkSettings.showInScene, GUILayout.Width(40));
                link.jumpLinkSettings.showInSceneDetailed = EditorGUILayout.ToggleLeft("Details", link.jumpLinkSettings.showInSceneDetailed, GUILayout.Width(55));
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    jumpLinks.RemoveAt(iLink);
                    SceneView.RepaintAll();
                    iLink--;
                    AssetDatabase.SaveAssets();
                    continue;
                }
                EditorGUILayout.EndHorizontal();
                if (link.jumpLinkSettings.showDetails)
                {
                    link.worldPointA = EditorGUILayout.Vector2Field("WorldPointA", link.worldPointA);
                    link.worldPointB = EditorGUILayout.Vector2Field("WorldPointB", link.worldPointB);
                    link.xSpeedScale = EditorGUILayout.Slider("Speed Percentage", link.xSpeedScale, 0, 1);
                    GUI.enabled = false;

                    EditorGUILayout.Vector2Field("NavPointA", link.navPosA.navPoint);
                    EditorGUILayout.Vector2Field("NavPointB", link.navPosB.navPoint);
                    EditorGUILayout.IntField("NodeIndexA", link.navPosA.navNodeIndex);
                    EditorGUILayout.IntField("NodeIndexB", link.navPosB.navNodeIndex);

                    GUI.enabled = true;
                }

                if (EditorGUI.EndChangeCheck())
                {
                    link.TryRemapPoints(navBuilder.GlobalBuildContainer.prebuildNavData);
                    link.RecalculateJumpArc(navBuilder.GlobalBuildContainer.navAgentSettings);
                    SceneView.RepaintAll();
                }
            }
            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("Add Link"))
            {
                ToogleShowInSceneDetailed(false);
                Camera[] sceneViewCams = SceneView.GetAllSceneCameras();
                if (sceneViewCams.Length > 0)
                {
                    Vector2 screenCenter = sceneViewCams[0].ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
                    jumpLinks.Add(new MetaJumpLink(screenCenter, screenCenter + Vector2.right * 3));
                }
                else
                {
                    jumpLinks.Add(new MetaJumpLink());
                }
                jumpLinks[jumpLinks.Count - 1].TryRemapPoints(navBuilder.GlobalBuildContainer.prebuildNavData);
                jumpLinks[jumpLinks.Count - 1].RecalculateJumpArc(navBuilder.GlobalBuildContainer.navAgentSettings);
                SceneView.RepaintAll();
            }
        }

        public void OnSceneGUI(SceneView sceneView)
        {
            NavData2DVisualizerWindow.SceneDrawNavData2D(navBuilder.GlobalBuildContainer.prebuildNavData);
            MetaJumpLink link;
            for (int iLink = 0; iLink < jumpLinks.Count; iLink++)
            {
                if (!jumpLinks[iLink].jumpLinkSettings.showInScene)
                    continue;


                link = jumpLinks[iLink];
                EditorGUI.BeginChangeCheck();

                link.DrawJumpLink(navBuilder.GlobalBuildContainer.navAgentSettings.width, navBuilder.GlobalBuildContainer.navAgentSettings.height
                    , link.jumpLinkSettings.showInSceneDetailed, link.isBiDirectional, true);

                if (EditorGUI.EndChangeCheck())
                {
                    link.TryRemapPoints(navBuilder.GlobalBuildContainer.prebuildNavData);
                    link.RecalculateJumpArc(navBuilder.GlobalBuildContainer.navAgentSettings);
                }
            }
        }

        public void OnSelectionChanges()
        {

        }

        void ToogleShowDetailAll(bool showDetail)
        {
            foreach (MetaJumpLink jl in jumpLinks)
            {
                jl.jumpLinkSettings.showDetails = showDetail;
            }
        }

        void ToogleShowInScene(bool showInScene)
        {
            foreach (MetaJumpLink jl in jumpLinks)
            {
                jl.jumpLinkSettings.showInScene = showInScene;
            }
            SceneView.RepaintAll();
        }

        void ToogleShowInSceneDetailed(bool showInSceneDetailed)
        {
            foreach (MetaJumpLink jl in jumpLinks)
            {
                jl.jumpLinkSettings.showInSceneDetailed = showInSceneDetailed;
            }
            SceneView.RepaintAll();
        }

        void ShowInvalidLinks()
        {
            foreach (MetaJumpLink jl in jumpLinks)
            {
                MetaJumpLink inverseLink = jl.InvertLink(navBuilder.GlobalBuildContainer.navAgentSettings);
                if (jl.jumpArc.j <= 0 || inverseLink.jumpArc.j <= 0)
                    jl.jumpLinkSettings.showInSceneDetailed = true;
                else
                    jl.jumpLinkSettings.showInSceneDetailed = false;
            }
        }
    }
}
