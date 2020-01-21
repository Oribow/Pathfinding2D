using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INavNode2d {
    Vector2 Position { get; }
    NavNodeConnection[] Connections { get; }
}

[System.Serializable]
public class NavNode2d : INavNode2d
{
    // assumption: 
    // segment nodes must have 0 - 2 connection
    // link nodes must have 3 connections. (0 = prev, 1 = next, |2 = link|)
    public Vector2 Position { get { return position; } }
    public NavNodeConnection Prev { get { return connections[0]; } set { connections[0] = value; } }
    public NavNodeConnection Next { get { return connections[1]; } set { connections[1] = value; } }
    public NavNodeConnection Link { get { return connections[2]; } set { connections[2] = value; } }
    public NavNodeConnection[] Connections { get { return connections; } }

    [SerializeField]
    private Vector2 position;
    [SerializeField]
    private NavNodeConnection[] connections;

    public NavNode2d(Vector2 pos, bool isThreeWay)
    {
        this.position = pos;
        this.connections = new NavNodeConnection[isThreeWay ? 3 : 2];
        for (int i = 0; i < this.Connections.Length; i++)
        {
            this.connections[i].costs = -1;
        }
    }
}

[System.Serializable]
public struct NavNodeConnection
{
    [System.NonSerialized]
    public NavNode2d node;
    // if costs < 0, then the connection is disabled and node could be null
    [SerializeField]
    public float costs;

    public NavNodeConnection(NavNode2d node, float costs)
    {
        this.node = node;
        this.costs = costs;
    }

    public bool IsEnabled()
    {
        return costs >= 0;
    }
}