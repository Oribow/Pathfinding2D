using UnityEngine;
using System.Collections;
using System;
using UnityEditor;
using NavMesh2D.Core;
using Utility;
using System.Collections.Generic;
using System.Linq;

namespace NavData2d.Editor
{
    [System.Serializable]
    public class NavDataGeometryBuilder : ITab
    {
        enum DebugOption { Outline, NavData, Unoptimized, None }
        public bool IsEnabled
        {
            get
            {
                return navBuilder.GlobalBuildContainer != null && navBuilder.GlobalBuildContainer.strippedContourTree != null;
            }
        }

        public string TabHeader
        {
            get
            {
                return "Tweaking";
            }
        }

        ContourTree strippedTree { get { return navBuilder.GlobalBuildContainer.strippedContourTree; } set { navBuilder.GlobalBuildContainer.strippedContourTree = value; } }

        INavDataBuilder navBuilder;
        [SerializeField]
        float minNodeLength;

        public NavDataGeometryBuilder(INavDataBuilder navBuilder)
        {
            this.navBuilder = navBuilder;
        }

        public void OnGUI()
        {
            minNodeLength = EditorGUILayout.FloatField("Min Node Length", minNodeLength);
            if (GUILayout.Button("Build"))
            {
                UpdateNavigationData2d();
            }
        }

        public void OnSceneGUI(SceneView sceneView)
        {
            if (navBuilder.GlobalBuildContainer.prebuildNavData)
            {
                NavData2DVisualizerWindow.SceneDrawNavData2D(navBuilder.GlobalBuildContainer.prebuildNavData);
            }
        }

        public void OnSelected()
        {

        }

        public void OnUnselected()
        {

        }

        void UpdateNavigationData2d()
        {
            var expandedTree = ExpandedTree.Build(strippedTree, navBuilder.GlobalBuildContainer.navAgentSettings.height);
            if (expandedTree != null)
            {
                if (navBuilder.GlobalBuildContainer.prebuildNavData == null)
                    navBuilder.GlobalBuildContainer.prebuildNavData = new NavigationData2D();
                new NavigationData2DBuilder(navBuilder.GlobalBuildContainer.navAgentSettings, minNodeLength).Build(expandedTree, navBuilder.GlobalBuildContainer.prebuildNavData);
                if (navBuilder.GlobalBuildContainer.filteredNavData == null)
                { //Throws null ref when serializing, because name and agent = null
                    navBuilder.GlobalBuildContainer.filteredNavData = MonoBehaviour.Instantiate<NavigationData2D>(navBuilder.GlobalBuildContainer.prebuildNavData);
                }
            }
            EditorUtility.SetDirty(navBuilder.GlobalBuildContainer.prebuildNavData);
            SceneView.RepaintAll();
        }
    }
}
