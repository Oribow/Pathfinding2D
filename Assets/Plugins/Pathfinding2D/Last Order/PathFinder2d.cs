using Priority_Queue;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder2d : MonoBehaviour
{
    public static PathFinder2d Instance { get { return instance; } }
    private static PathFinder2d instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("Multiple PathFinder2d components detected! Only 1 is allowed.");
        }

        instance = this;
    }
}

class AStar {
    FastPriorityQueue<PathNode> openList;
    List<PathNode> closedNodes;

    public NavPath2d FindPath(NavPosition2d start, NavPosition2d end)
    {
        var path = new NavPath2d();

        closedNodes.Clear();
        openList.Clear();

        PathNode startNode = new PathNode(segmentOfStart, 0);
        openList.Enqueue(startNode, 0);

        while (openList.Count > 0)
        {
            var node = openList.Dequeue();
            if (node.navNode == end)
            { 
                // found path to correct node

            }

            closedNodes.Add(node);

        }
    }

    private float Costs(Vector2 pos, Vector2 goal)
    {
        return Vector2.Distance(pos, goal);
    }
}

class PathNode : FastPriorityQueueNode
{
    public float costSoFar;
    public PathNode parent;
    public NavSegment targetSegment;

    public PathNode(INavNode navNode, float costSoFar) {
        this.costSoFar = costSoFar;
        this.navNode = navNode;
    }
}