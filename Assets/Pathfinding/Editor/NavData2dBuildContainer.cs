using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using NavData2d.Editor;

namespace Pathfinding2d.NavDataGeneration
{
    internal class NavData2dBuildContainer : ScriptableObject
    {
        [SerializeField]
        internal NavAgentGroundWalkerSettings navAgentSettings;
        [SerializeField]
        internal ColliderSet colliderSet;
        [SerializeField]
        ContourTree contourTree;
        [SerializeField]
        internal ContourTree strippedContourTree;
        [SerializeField]
        internal NavigationData2D prebuildNavData;
        [SerializeField]
        internal NavigationData2D filteredNavData;
        [SerializeField]
        internal List<MetaJumpLink> jumpLinks;
        [SerializeField]
        internal float nodeMergeDistance;
        [SerializeField]
        internal float maxEdgeDeviation;
        //DebugVertSet 

#if UNITY_EDITOR
        public void SaveToAsset()
        {
            string path = EditorUtility.SaveFilePanel("Save NavData2DBuildContainer", "Assets", "NavData2DBuildContainer", "asset");
            if (path == null || path.Length == 0)
                return;
            path = path.Substring(path.IndexOf("Assets"));
            Debug.Log(path);
            AssetDatabase.CreateAsset(this, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = this;
        }

        public void UpdateAssetRef ()
        {
            if (!AssetDatabase.Contains(navAgentSettings))
                AssetDatabase.AddObjectToAsset(navAgentSettings, this);

            if (!AssetDatabase.Contains(prebuildNavData))
                AssetDatabase.AddObjectToAsset(prebuildNavData, this);

            if (!AssetDatabase.Contains(filteredNavData))
                AssetDatabase.AddObjectToAsset(filteredNavData, this);
        }
#endif
    }
}
