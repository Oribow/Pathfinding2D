using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using NavData2d.Editor;

namespace NavGraph.Build
{
    [System.Serializable]
    public class BuildProcessSave : MonoBehaviour
    {
        public ColliderSet ColliderSet
        {
            get
            {
                if (colliderSet == null)
                    colliderSet = new ColliderSet();
                return colliderSet;
            }
        }

        public ContourTreeBuilderData ContourTreeBuilderData
        {
            get
            {
                if (contourTreeBuilderData == null)
                {
                    contourTreeBuilderData = new ContourTreeBuilderData();
                }
                return contourTreeBuilderData;
            }
        }

        public ContourTree VanilaContourTree
        {
            get
            {
                if (vanilaContourTree == null)
                {
                    vanilaContourTree = new ContourTree();
                }
                return vanilaContourTree;
            }
            set
            {
                vanilaContourTree = value;
            }
        }

        public ContourTree OptimizedContourTree
        {
            get
            {
                if (optimizedContourTree == null)
                {
                    optimizedContourTree = new ContourTree();
                }
                return optimizedContourTree;
            }
            set
            {
                optimizedContourTree = value;
            }
        }

        public ContourTree StrippedContourTree
        {
            get
            {
                if (strippedContourTree == null)
                {
                    strippedContourTree = new ContourTree();
                }
                return strippedContourTree;
            }
            set
            {
                strippedContourTree = value;
            }
        }

        public Vector3[][] VanilaContourTreeVerts
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

        public Vector3[][] OptimizedContourTreeVerts
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

        [SerializeField]
        ColliderSet colliderSet;
        [SerializeField]
        NavAgentGroundWalkerSettings navAgentSettings;
        [SerializeField]
        ContourTree vanilaContourTree;
        [SerializeField]
        ContourTree optimizedContourTree;
        [SerializeField]
        ContourTree strippedContourTree;
        [SerializeField]
        NavigationData2D prebuildNavData;
        [SerializeField]
        NavigationData2D filteredNavData;
        [SerializeField]
        ContourTreeBuilderData contourTreeBuilderData;

        public List<MetaJumpLink> jumpLinks;


        public BuildProcessSave(string path)
        {
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

            OptimizedContourTree = Utility.OribowsUtilitys.DeepCopy(VanilaContourTree, 
                new Utility.OribowsUtilitys.SerializationSurrogateContainer(new Vector2SerializationSurrogate(), typeof(Vector2), new System.Runtime.Serialization.StreamingContext(System.Runtime.Serialization.StreamingContextStates.All)),
                new Utility.OribowsUtilitys.SerializationSurrogateContainer(new BoundsSerializationSurrogate(), typeof(Bounds), new System.Runtime.Serialization.StreamingContext(System.Runtime.Serialization.StreamingContextStates.All)));
            foreach (var node in OptimizedContourTree)
                node.contour.Optimize(ContourTreeBuilderData.nodeMergeDistance, ContourTreeBuilderData.maxEdgeDeviation);
            OptimizedContourTreeVerts = OptimizedContourTree.ToVertexArray();
        }

        public static BuildProcessSave CreateNewInstance(string name)
        {
            GameObject g = new GameObject(name);
            g.hideFlags = HideFlags.DontSaveInBuild;
            var component = g.AddComponent<BuildProcessSave>();
            return component;
        }
    }
}
