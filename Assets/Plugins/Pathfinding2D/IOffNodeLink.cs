using UnityEngine;
using System;
using NavGraph;

[Serializable]
public abstract class IOffNodeLink
{
    public NavPosition targetPos;
    public Vector2 startPoint;
    public float traversCosts;
}
