using UnityEditor;
using Utility;

namespace NavGraph
{
    [CustomEditor(typeof(NavAgentType))]
    public class NavAgentGroundWalkerSettingAsset : Editor
    {

        [MenuItem("Pathfinding/NavAgentSettings/GroundWalker")]
        public static void CreateAsset()
        {
            ScriptableObjectUtility.CreateAsset<NavAgentType>("GroundWalkerSetting");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}
