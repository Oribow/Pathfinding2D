using UnityEngine;

namespace NavGraph
{
    /// <summary>
    /// Stores all information necessary to build NavData for a moving Agent.
    /// </summary>
    public class NavAgentGroundWalkerSettings : ScriptableObject
    {
        public float height;
        public float width;
        [Range(0.01f, 100)]
        public float maxXVel = 1;
        public float slopeLimit;
    }
}
