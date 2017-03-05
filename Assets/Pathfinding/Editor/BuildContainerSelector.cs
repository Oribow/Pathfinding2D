using UnityEngine;
using System.Collections;
using System;
using UnityEditor;

namespace Pathfinding2d.NavDataGeneration
{
    [System.Serializable]
    public class BuildContainerSelector : ITab
    {
        INavDataBuilder navBuilder;

        public bool IsEnabled
        {
            get
            {
                return true;
            }
        }

        public string TabHeader
        {
            get
            {
                return "ContSelector";
            }
        }

        public BuildContainerSelector(INavDataBuilder navBuilder)
        {
            this.navBuilder = navBuilder;
        }

        public void OnGUI()
        {
            navBuilder.GlobalBuildContainer = (NavData2dBuildContainer)EditorGUILayout.ObjectField(navBuilder.GlobalBuildContainer, typeof(NavData2dBuildContainer), true);

            if (GUILayout.Button("Create New"))
            {
                navBuilder.GlobalBuildContainer = new NavData2dBuildContainer();
                navBuilder.GlobalBuildContainer.SaveToAsset();
            }
            GUI.enabled = true;
        }

        public void OnSceneGUI(SceneView sceneView)
        {
            
        }

        public void OnSelected()
        {
        
        }

        public void OnUnselected()
        {
            
        }
    }
}
