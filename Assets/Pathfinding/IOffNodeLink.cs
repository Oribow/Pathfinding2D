using UnityEngine;
using System;
using Pathfinding2d;

[Serializable]
public abstract class IOffNodeLink
{
    public NavPosition targetPos;
    public Vector2 startPoint;
    public float traversCosts;
}
