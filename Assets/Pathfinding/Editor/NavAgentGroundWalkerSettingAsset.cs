using UnityEngine;
using System.Collections;
using UnityEditor;
using Utility;

[CustomEditor(typeof(NavAgentGroundWalkerSettings))]
public class NavAgentGroundWalkerSettingAsset : Editor
{

    [MenuItem("Pathfinding/NavAgentSettings/GroundWalker")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<NavAgentGroundWalkerSettings>("GroundWalkerSetting");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
