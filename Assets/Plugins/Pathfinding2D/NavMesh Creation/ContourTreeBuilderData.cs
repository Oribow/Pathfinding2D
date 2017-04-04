using UnityEngine;

namespace NavGraph.Build
{
    [System.Serializable]
    public class ContourTreeBuilderData
    {
        public float nodeMergeDistance;
        public float maxEdgeDeviation;
        public Vector3[][] unoptimizedTreeVerts;
        public Vector3[][] optimizedTreeVerts;
    }
}
