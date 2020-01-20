using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INavNode
{
    List<NavNodeConnection> Connections();
}

public class NavNode : INavNode
{
    public NavNodeConnection A { get { return connections[0]; } set { connections[0] = value; } }
    public NavNodeConnection B { get { return connections[1]; } set { connections[1] = value; } }

    private Vector2 position;
    private NavNodeConnection[] connections;

    public NavNode(Vector2 position, NavNodeConnection a, NavNodeConnection b)
    {
        this.position = position;
        this.connections = new NavNodeConnection[2];
        connections[0] = a;
        connections[1] = b;
    }

    public NavNodeConnection[] Connections()
    {
        return connections;
    }
}

public class NavNodeLink : INavNode
{
    public NavNodeConnection A { get { return connections[0]; } set { connections[0] = value; } }
    public NavNodeConnection B { get { return connections[1]; } set { connections[1] = value; } }
    public NavNodeConnection Link { get { return connections[2]; } set { connections[2] = value; } }

    private Vector2 position;
    private NavNodeConnection[] connections;

    public NavNodeLink(Vector2 position, NavNodeConnection a, NavNodeConnection link, NavNodeConnection b)
    {
        this.position = position;
        connections = new NavNodeConnection[3];
        connections[0] = a;
        connections[1] = b;
        connections[2] = link;
    }

    public NavNodeConnection[] Connections()
    {
        return connections;
    }
}

public struct NavNodeConnection
{
    public INavNode end;
    public float costs;

    public NavNodeConnection(INavNode end, float costs)
    {
        this.end = end;
        this.costs = costs;
    }
}