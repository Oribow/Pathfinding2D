using UnityEngine;
using System.Collections;
using System;

public class NavAgentGroundWalkerSettings : ScriptableObject {

    public float height;
    public float width;
    public float jumpForce;
    public float gravity;
    [Range (0.01f, 100)]
    public float maxXVel = 1;
    public float slopeLimit;

}
