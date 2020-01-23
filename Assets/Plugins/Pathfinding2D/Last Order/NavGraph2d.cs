using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this graph is read only, all changes have to come from their source objects
public class NavGraph2d
{
    public static NavGraph2d Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod]
    static void Initialize()
    {
        NavGraph2d.Instance = new NavGraph2d();
    }

    private FreeList<NavNode2d> navNodes;

    public NavGraph2d()
    {
        navNodes = new FreeList<NavNode2d>(null, 100);

        // this is the null element, used for null checks. Its index is 0
        navNodes.Add(new NavNode2d(Vector2.zero, false));
    }

    public int AddNode(NavNode2d newNode)
    {
        return navNodes.Add(newNode);
    }

    public void RemoveNode(int index)
    {
        navNodes.Remove(index);
    }

    public void RemoveLinkNode(int index, NavSegment segment)
    {
        var node = navNodes[index];
        var nextNode = navNodes[node.Next.goalNodeIndex];
        var prevNode = navNodes[node.Prev.goalNodeIndex];

        float costs = segment.GetCosts(Vector2.Distance(nextNode.Position, prevNode.Position));
        prevNode.Next = new NavNodeConnection(node.Next.goalNodeIndex, costs);
        nextNode.Prev = new NavNodeConnection(node.Prev.goalNodeIndex, costs);

        RemoveNode(index);
    }

    public int AddLinkNode(Vector2 position, NavSegment segment)
    {
        float distanceSqr = (position - segment.Start).sqrMagnitude;
        Debug.Assert(distanceSqr <= segment.Length * segment.Length);

        int nIndex = segment.navNodeIndex;
        NavNode2d n = navNodes[nIndex];
        do
        {
            nIndex = n.Next.goalNodeIndex;
            n = navNodes[nIndex];
        } while (distanceSqr > (n.Position - segment.Start).sqrMagnitude);

        // prev(l) <--> newlink <--> n
        var prev = navNodes[n.Prev.goalNodeIndex];
        var next = n;

        float costsToNext = segment.GetCosts(Vector2.Distance(next.Position, position));
        float costsToPrev = segment.GetCosts(Vector2.Distance(prev.Position, position));

        var newNode = new NavNode2d(position, true);
        int index = navNodes.Add(newNode);

        prev.Next = new NavNodeConnection(index, costsToPrev);
        next.Prev = new NavNodeConnection(index, costsToNext);

        newNode.Prev = new NavNodeConnection(n.Prev.goalNodeIndex, costsToPrev);
        newNode.Next = new NavNodeConnection(nIndex, costsToNext);

        return index;
    }

    public NavNode2d GetNode(int index)
    {
        return navNodes[index];
    }

    public void DrawGizmos()
    {
        foreach (var node in navNodes)
        {
            if (node.Next.IsEnabled())
            {
                Gizmos.color = ABC.Utility.LinearBlendFromGreenToYellowToRed(node.Next.costs / 10f);
                ABC.Utility.DrawBezierConnection(node.Position, navNodes[node.Next.goalNodeIndex].Position, false);
            }
            if (node.Prev.IsEnabled())
            {
                Gizmos.color = ABC.Utility.LinearBlendFromGreenToYellowToRed(node.Prev.costs / 10f);
                ABC.Utility.DrawBezierConnection(node.Position, navNodes[node.Prev.goalNodeIndex].Position, false);
            }
        }
    }
}
