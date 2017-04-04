using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using NavData2d.Editor;

namespace NavGraph.Build
{
    [System.Serializable]
    public class BuildProcessSave : ScriptableObject
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
                    contourTreeBuilderData = CreateAsset<ContourTreeBuilderData>();
                }
                return contourTreeBuilderData;
            }
        }

        internal ContourTree VanilaContourTree
        {
            get
            {
                if (ContourTreeBuilderData.vanilaContourTree == null)
                {
                    ContourTreeBuilderData.vanilaContourTree = CreateAsset<ContourTree>();
                }
                return ContourTreeBuilderData.vanilaContourTree;
            }
            set
            {
                if (ContourTreeBuilderData.vanilaContourTree == null)
                {
                    ContourTreeBuilderData.vanilaContourTree = value;
                    AssetDatabase.AddObjectToAsset(ContourTreeBuilderData.vanilaContourTree, assetPath);
                }
                else
                {
                    ScriptableObject.DestroyImmediate(ContourTreeBuilderData.vanilaContourTree, true);
                    ContourTreeBuilderData.vanilaContourTree = value;
                    AssetDatabase.AddObjectToAsset(ContourTreeBuilderData.vanilaContourTree, assetPath);
                    AssetDatabase.SaveAssets();
                }
            }
        }

        internal ContourTree OptimizedContourTree
        {
            get
            {
                if (ContourTreeBuilderData.optimizedContourTree == null)
                {
                    ContourTreeBuilderData.optimizedContourTree = CreateAsset<ContourTree>();
                }
                return ContourTreeBuilderData.optimizedContourTree;
            }
            set
            {
                if (ContourTreeBuilderData.optimizedContourTree == null)
                {
                    ContourTreeBuilderData.optimizedContourTree = value;
                    AssetDatabase.AddObjectToAsset(ContourTreeBuilderData.optimizedContourTree, assetPath);
                }
                else
                {
                    ScriptableObject.DestroyImmediate(ContourTreeBuilderData.optimizedContourTree, true);
                    ContourTreeBuilderData.optimizedContourTree = value;
                    AssetDatabase.AddObjectToAsset(ContourTreeBuilderData.optimizedContourTree, assetPath);
                    AssetDatabase.SaveAssets();
                }
            }
        }

        internal ContourTree StrippedContourTree
        {
            get
            {
                if (strippedContourTree == null)
                {
                    strippedContourTree = CreateAsset<ContourTree>();
                }
                return strippedContourTree;
            }
            set
            {
                if (strippedContourTree == null)
                {
                    strippedContourTree = value;
                    AssetDatabase.AddObjectToAsset(strippedContourTree, assetPath);
                }
                else
                {
                    ScriptableObject.DestroyImmediate(strippedContourTree, true);
                    strippedContourTree = value;
                    AssetDatabase.AddObjectToAsset(strippedContourTree, assetPath);
                    AssetDatabase.SaveAssets();
                }
            }
        }

        internal Vector3[][] VanilaContourTreeVerts
        {
            get
            {
                if (ContourTreeBuilderData.unoptimizedTreeVerts == null)
                    ContourTreeBuilderData.unoptimizedTreeVerts = VanilaContourTree.ToVertexArray();

                return ContourTreeBuilderData.unoptimizedTreeVerts;
            }
            set
            {
                ContourTreeBuilderData.unoptimizedTreeVerts = value;
            }
        }

        internal Vector3[][] OptimizedContourTreeVerts
        {
            get
            {
                if (ContourTreeBuilderData.optimizedTreeVerts == null)
                    ContourTreeBuilderData.optimizedTreeVerts = OptimizedContourTree.ToVertexArray();
                return ContourTreeBuilderData.optimizedTreeVerts;
            }
            set
            {
                ContourTreeBuilderData.optimizedTreeVerts = value;
            }
        }

        [System.NonSerialized]
        ColliderSet colliderSet;

        [SerializeField]
        NavAgentGroundWalkerSettings navAgentSettings;
        [SerializeField]
        ContourTree strippedContourTree;
        [SerializeField]
        NavigationData2D prebuildNavData;
        [SerializeField]
        NavigationData2D filteredNavData;
        [SerializeField]
        ContourTreeBuilderData contourTreeBuilderData;

        public List<MetaJumpLink> jumpLinks;

        [System.NonSerialized]
        public string assetPath;
        [System.NonSerialized]
        public string assetName;

        public BuildProcessSave(string path)
        {
            assetPath = path;
            assetName = path.Substring(path.LastIndexOf("/") + 1);

            //Create an Assetfile of this class, if it doesn't exist already.
            if (!System.IO.File.Exists(System.IO.Path.Combine(Application.dataPath.Remove(Application.dataPath.LastIndexOf("/")), assetPath)))
            {
                CreateMainAsset();
            }
        }

        public void BuildVanilaContourTree()
        {
            if (ColliderSet.colliderList.Count == 0)
                return;

            var collisionGeometrySet = ColliderSet.ToCollisionGeometrySet();
            VanilaContourTree = ContourTree.Build(collisionGeometrySet);
            VanilaContourTreeVerts = VanilaContourTree.ToVertexArray();
        }

        public void OptimizeVanilaContourTree()
        {
            if (VanilaContourTree.ContourCount() == 0)
                return;

            OptimizedContourTree = ScriptableObject.Instantiate<ContourTree>(VanilaContourTree);
            foreach (var node in OptimizedContourTree)
                node.contour.Optimize(ContourTreeBuilderData.nodeMergeDistance, ContourTreeBuilderData.maxEdgeDeviation);
            OptimizedContourTreeVerts = OptimizedContourTree.ToVertexArray();
        }

        T CreateAsset<T>() where T : ScriptableObject
        {
            T asset;
            if (!System.IO.File.Exists(System.IO.Path.Combine(Application.dataPath.Remove(Application.dataPath.LastIndexOf("/")), assetPath)))
            {
                CreateMainAsset();

            }
            asset = ScriptableObject.CreateInstance<T>();
            //asset.hideFlags = HideFlags.NotEditable | HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(asset, assetPath);
            return asset;
        }

        void CreateMainAsset()
        {
            this.hideFlags = HideFlags.NotEditable;
            AssetDatabase.CreateAsset(this, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = this;
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
