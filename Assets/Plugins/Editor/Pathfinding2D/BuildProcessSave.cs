using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using NavData2d.Editor;

namespace NavGraph.Build
{
    [System.Serializable]
    public class BuildProcessSave
    {
        internal ColliderSet ColliderSet
        {
            get
            {
                if (colliderSet == null)
                    LoadColliderSet();
                return colliderSet;
            }
        }

        internal ContourTreeBuilderData ContourTreeBuilderData
        {
            get
            {
                if (contourTreeBuilderData == null)
                {
                    contourTreeBuilderData = LoadAsset<ContourTreeBuilderData>();
                }
                return contourTreeBuilderData;
            }
        }

        internal ContourTree ContourTree
        {
            get
            {
                if (contourTree == null)
                {
                    contourTree = LoadAsset<ContourTree>();
                }
                return contourTree;
            }
            set
            {
                if (contourTree == null)
                {
                    contourTree = value;
                    AssetDatabase.AddObjectToAsset(contourTree, assetPath);
                }
                else
                {
                    ScriptableObject.DestroyImmediate(contourTree, true);
                    contourTree = value;
                    AssetDatabase.AddObjectToAsset(contourTree, assetPath);
                    AssetDatabase.SaveAssets();
                }
            }
        }

        /*
        internal ContourTree StrippedContourTree
        {
            get
            {
                if (strippedContourTree == null)
                {
                    strippedContourTree = ScriptableObject.CreateInstance<ContourTree>();
                    AssetDatabase.AddObjectToAsset(strippedContourTree, assetPath);
                }
                return strippedContourTree;
            }
        }*/
        [SerializeField]
        internal ContourTree unoptimizedTree;
        [SerializeField]
        internal Vector3[][] unoptimizedTreeVerts;
        [SerializeField]
        internal Vector3[][] optimizedTreeVerts;

        [SerializeField]
        NavAgentGroundWalkerSettings navAgentSettings;
        [SerializeField]
        ColliderSet colliderSet;
        [SerializeField]
        ContourTree contourTree;
        [SerializeField]
        ContourTree strippedContourTree;
        [SerializeField]
        NavigationData2D prebuildNavData;
        [SerializeField]
        NavigationData2D filteredNavData;
        [SerializeField]
        ContourTreeBuilderData contourTreeBuilderData;
        public List<MetaJumpLink> jumpLinks;


        public string assetPath;
        public string assetName;

        public BuildProcessSave(string path)
        {
            assetPath = path;
            assetName = path.Substring(path.LastIndexOf("/") + 1);
        }

        public void RebuildContourTree()
        {
            var collisionGeometrySet = ColliderSet.ToCollisionGeometrySet();
            unoptimizedTree = ContourTree.Build(collisionGeometrySet);
            unoptimizedTreeVerts = unoptimizedTree.ToVertexArray();
        }

        public void TweakContourTree()
        {
            ContourTree = ScriptableObject.Instantiate<ContourTree>(unoptimizedTree);

            foreach (var node in ContourTree)
                node.contour.Optimize(ContourTreeBuilderData.nodeMergeDistance, ContourTreeBuilderData.maxEdgeDeviation);
            optimizedTreeVerts = contourTree.ToVertexArray();
            SceneView.RepaintAll();
        }

        T LoadAsset<T>() where T : ScriptableObject
        {
            T asset;
            if (!System.IO.File.Exists(System.IO.Path.Combine(Application.dataPath.Remove(Application.dataPath.LastIndexOf("/")), assetPath)))
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = asset;

            }
            else
            {
                asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<T>();
                    AssetDatabase.AddObjectToAsset(asset, assetPath);
                }
            }
            return asset;
        }

        void LoadColliderSet()
        {
            //Lets do a quick sanity check and clear up dead data
            ColliderSet[] sets = GameObject.FindObjectsOfType<ColliderSet>();
            if (sets.Length > 0)
            {
                if (sets.Length > 1) // AH-HA! Dead data found!
                {
                    for (int i = 1; i < sets.Length; i++)
                        GameObject.DestroyImmediate(sets[i]);
                }
                //Make sure it uses the correct hideflags.
                sets[0].hideFlags = HideFlags.DontUnloadUnusedAsset | HideFlags.DontSaveInBuild;
            }


            colliderSet = GameObject.FindObjectOfType<ColliderSet>();
            if (colliderSet == null)
            {
                colliderSet = new GameObject("ColliderSet")
                { hideFlags = HideFlags.DontUnloadUnusedAsset | HideFlags.DontSaveInBuild }
                .AddComponent<ColliderSet>();
            }
        }

#if UNITY_EDITOR
        public static BuildProcessSave SaveAsNewAsset()
        {
            string path = EditorUtility.SaveFilePanel("Save NavData2DBuildContainer", "Assets", "NavData2DBuildContainer", "asset");
            if (path == null || path.Length == 0)
                return null;
            path = path.Substring(path.IndexOf("Assets"));
            Debug.Log("Saved Container to: " + path);
            BuildProcessSave container = new BuildProcessSave(path);
            return container;
        }

        public static BuildProcessSave SelectExistingAsset()
        {
            string path = EditorUtility.OpenFilePanelWithFilters("Open BuildContainer", "Assets", new string[] { "Asset", "asset" });
            if (path == null || path.Length == 0)
                return null;
            path = path.Substring(path.IndexOf("Assets"));
            return new BuildProcessSave(path);
        }
#endif
    }
}
