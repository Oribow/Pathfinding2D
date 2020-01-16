using UnityEngine;

/// <summary>
/// Stores all information necessary to build NavData for a moving Agent.
/// </summary>
[CreateAssetMenu(fileName = "NavAgentType", menuName = "Nav2d/NavAgentType", order = 1)]
public class NavAgentType : ScriptableObject
{
    public string typeName;
    public float height;
    public float width;
    public float slopeLimit;
    public bool gravityBound;
}

