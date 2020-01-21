using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NavPosition2d : INavNode2d
{
    public NavNodeConnection Prev { get { return connections[0]; } set { connections[0] = value; } }
    public NavNodeConnection Next { get { return connections[1]; } set { connections[1] = value; } }
    public NavNodeConnection[] Connections { get { return connections; } }
    public Vector2 Position { get { return position; } }
    public Vector2 Tangent { get { return new Vector2(normal.y, -normal.x); } }
    public Vector2 Normal { get { return normal; } }

    [SerializeField]
    private NavNodeConnection[] connections;
    [SerializeField]
    private Vector2 position;
    [SerializeField]
    private Vector2 normal;

    public NavPosition2d(Vector2 position, Vector2 normal, NavNodeConnection prev, NavNodeConnection next)
    {
        this.connections = new NavNodeConnection[2];
        this.Prev = prev;
        this.Next = next;
        this.position = position;
        this.normal = normal;
    }
}
