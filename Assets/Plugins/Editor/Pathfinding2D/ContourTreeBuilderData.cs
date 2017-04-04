using UnityEngine;

namespace NavGraph.Build
{
    class ContourTreeBuilderData : ScriptableObject
    {
        public float nodeMergeDistance;
        public float maxEdgeDeviation;
        public ContourTree vanilaContourTree;
        public ContourTree optimizedContourTree;
        public Vector3[][] unoptimizedTreeVerts;
        public Vector3[][] optimizedTreeVerts;
    }
}
