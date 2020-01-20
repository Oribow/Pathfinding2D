using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NavPosition2d
{
    public INavNode NavNode { get { return navNode; } }
    public Vector2 Point { get { return navNode.GetPosition(t); } }

    public float PositionOnNode { get { return t; } }

    [SerializeField]
    private INavNode navNode;
    [SerializeField]
    private float t;

    public NavPosition2d(INavNode navNode, float t)
    {
        this.navNode = navNode;
        this.t = t;
    }
}
