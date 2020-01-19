using UnityEngine;

/// <summary>
/// Stores all information necessary to build NavData for a moving Agent.
/// </summary>
[CreateAssetMenu(fileName = "NavAgentType", menuName = "Nav2d/NavAgentType", order = 1)]
[System.Serializable]
public class NavAgentType : ScriptableObject
{
    public string typeName;
    public float height;
    public float width;
    public float slopeLimit;
    public float maxXVelocity;
    public float gravity;
    public float jumpForce;
    public bool gravityBound;

    public bool CalculateJumpArc(Vector2 start, Vector2 end, out float velocity, out float jumpForce) {
        velocity = Mathf.Abs(end.x - start.x) / maxXVelocity;
        jumpForce = gravity * velocity * 0.5f + (end.y - start.y) / velocity;

        if (Mathf.Abs(jumpForce) > this.jumpForce || jumpForce < 0)
            return false;
        else
            return true;
    }
}

