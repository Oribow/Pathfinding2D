using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Priority_Queue;
using Pathfinding2D;
using System;
using NavData2d;

public class PathPlaner : MonoBehaviour
{

    public static PathPlaner Instance { get { return instance; } }
    private static PathPlaner instance;
#if false
    [SerializeField]
    NavigationData2D navData;

    void Awake()
    {
        Debug.Assert(instance == null);
        instance = this;
        openList = new StablePriorityQueue<IPathNode>(100);
        closedNodes = new List<IPathNode>(100);
        closedLinks = new List<IOffNodeLink>(100);
        requestBuffer = new Stack<PathRequest>(5);
    }

    Stack<PathRequest> requestBuffer;

    public NavPosition MapPoint(Vector2 unmappedPoint)
    {
        NavPosition result;
        navData.SamplePoint(unmappedPoint, out result);
        return result;
    }

    public void RequestPath(PathRequest request)
    {
        requestBuffer.Push(request);
    }

    StablePriorityQueue<IPathNode> openList;
    List<IPathNode> closedNodes;
    List<IOffNodeLink> closedLinks;

    public void FindRequestedPath(PathRequest request)
    {
        Debug.Assert(request.goal.navNodeIndex != -1);
        Debug.Assert(request.start.navNodeIndex != -1);

        //trivial case: Start and Goal are on the same line
        float mult;
        if (request.start.navNodeIndex == request.goal.navNodeIndex && request.start.navVertIndex == request.goal.navVertIndex && IsLineTraversable(request, request.start.navPoint, request.goal.navPoint, out mult))
        {
            request.callback(new NavPath(new IPathSegment[] { new PathSegment(request.start.navPoint, request.goal.navPoint, Vector2.Distance(request.start.navPoint, request.goal.navPoint) / navData.navAgentSettings.maxXVel) }));
            return;
        }
        closedNodes.Clear();
        closedLinks.Clear();
        openList.Clear();

        IPathNode cNode = new PathNode_Start(navData.nodes[request.start.navNodeIndex], request.start.navVertIndex, Vector2.Distance(navData.nodes[request.start.navNodeIndex].verts[request.start.navVertIndex].PointB, request.goal.navPoint), request.start.navPoint);
        openList.Enqueue(cNode, cNode.totalCost);
        float costSoFar;
        float costMultiplier = 1;
        Vector2 previousPoint = request.start.navPoint;

        while (openList.Count > 0)
        {
            cNode = openList.Dequeue();
            if (cNode.navNode == navData.nodes[request.goal.navNodeIndex] && cNode.navVertIndex == request.goal.navVertIndex && IsLineTraversable(request, cNode.StartPoint, request.goal.navPoint, out costMultiplier))
            {
                goto FoundPath;
            }

            closedNodes.Add(cNode);

            int neighbourLeft, neighbourRight;
            cNode.GetNeighbours(out neighbourLeft, out neighbourRight);


            if (neighbourLeft != -1 && cNode.navNode.verts[neighbourLeft].distanceBC != 0)
            {
                if (!IsNodeClosed(cNode.navNode, neighbourLeft) && IsLineTraversable(request, cNode.StartPoint, cNode.NavVert.PointB, out costMultiplier))
                {
                    costSoFar = cNode.costSoFar + cNode.navNode.verts[neighbourLeft].distanceBC;
                    IPathNode newNode = new PathNode_Edge(cNode, cNode.navNode, neighbourLeft, false, costSoFar, Vector2.Distance(request.goal.navPoint, cNode.navNode.verts[neighbourLeft].PointB));
                    openList.Enqueue(newNode, newNode.costSoFar);
                }
            }

            if (neighbourRight != -1 && cNode.navNode.verts[neighbourLeft].distanceBC != 0)
            {
                if (!IsNodeClosed(cNode.navNode, neighbourRight) && IsLineTraversable(request, cNode.StartPoint, cNode.navNode.verts[neighbourRight].PointB, out costMultiplier))
                {
                    costSoFar = cNode.costSoFar + cNode.NavVert.distanceBC * costMultiplier;
                    IPathNode newNode = new PathNode_Edge(cNode, cNode.navNode, neighbourRight, true, costSoFar, Vector2.Distance(request.goal.navPoint, cNode.navNode.verts[neighbourRight].PointB));
                    openList.Enqueue(newNode, newNode.costSoFar);
                }
            }


            if (cNode.NavVert.linkIndex.Length > 0)
            {
                int[] linkIndecies = cNode.NavVert.linkIndex;
                for (int iLink = 0; iLink < linkIndecies.Length; iLink++)
                {
                    IOffNodeLink cLink = cNode.navNode.links[linkIndecies[iLink]];
                    if (!IsLinkClosed(cLink) && IsLineTraversable(request, cNode.StartPoint, cLink.startPoint, out costMultiplier))
                    {
                        costSoFar = cNode.costSoFar + cLink.traversCosts;
                        IPathNode newNode = new PathNode_Link(cNode, navData.nodes[cLink.targetPos.navNodeIndex], cLink.targetPos.navVertIndex, costSoFar, Vector2.Distance(request.goal.navPoint, cLink.targetPos.navPoint), linkIndecies[iLink]);
                        openList.Enqueue(newNode, newNode.costSoFar);
                        closedLinks.Add(cLink);
                    }
                }
            }

            request.StartsInNotWalkableLight = false;
        }

        request.callback(null);
        Debug.Log("No Path found");
        return;

        FoundPath:

        List<IPathSegment> pathSegments = new List<IPathSegment>(50);
        Vector2 prevPoint = request.goal.navPoint;

        while (cNode.parent != null)
        {
            if (cNode.GetType() == typeof(PathNode_Edge))
            {
                pathSegments.Add(new PathSegment(cNode.EndPoint, prevPoint, float.MaxValue));
                prevPoint = cNode.EndPoint;
            }
            else//link
            {
                var linkNode = (PathNode_Link)cNode;
                pathSegments.Add(new PathSegment(linkNode.Link.targetPos.navPoint, prevPoint, float.MaxValue));
                prevPoint = cNode.EndPoint;
                pathSegments.Add(new JumpSegment((JumpLink)linkNode.Link));
                prevPoint = linkNode.Link.startPoint;
            }
            cNode = cNode.parent;
        }
        pathSegments.Add(new PathSegment(request.start.navPoint, prevPoint, float.MaxValue));

        IPathSegment[] inversedSeg = new IPathSegment[pathSegments.Count];
        for (int iSeg = 0, iInv = pathSegments.Count - 1; iSeg < pathSegments.Count; iSeg++, iInv--)
        {
            inversedSeg[iSeg] = pathSegments[iInv];
        }
        request.callback(new NavPath(inversedSeg));
    }

    public void Flee(FleeRequest request)
    {
        Debug.Assert(request.start.navNodeIndex != -1);
        Debug.Assert(request.threats.Length != 0);

        closedNodes.Clear();
        closedLinks.Clear();
        openList.Clear();

        IPathNode cNode = new PathNode_Start(navData.nodes[request.start.navNodeIndex], request.start.navVertIndex, CalculateDistanceFromThreats(request, request.start.navPoint), request.start.navPoint);
        //Force the algo to take a node that will not cross the threat

        openList.Enqueue(cNode, cNode.totalCost);
        float costSoFar;
        float costMultiplier = 1;
        Vector2 previousPoint = request.start.navPoint;
        int forceDirection = (request.threats[0].x - request.start.navPoint.x) > 0 ? 1 : -1;

        while (openList.Count > 0)
        {
            cNode = openList.Dequeue();

            if (cNode.costSoFar >= request.targetDistanceFromThreats)
                break;

            closedNodes.Add(cNode);

            int neighbourLeft, neighbourRight;
            cNode.GetNeighbours(out neighbourLeft, out neighbourRight);


            if (neighbourLeft != -1 && forceDirection <= 0/*&& cNode.navNode.verts[neighbourLeft].distanceBC != 0*/)
            {
                if (!IsNodeClosed(cNode.navNode, neighbourLeft) && IsLineTraversable(request, cNode.StartPoint, cNode.NavVert.PointB, out costMultiplier))
                {
                    costSoFar = CalculateDistanceFromThreats(request, cNode.navNode.verts[neighbourLeft].PointB);
                    IPathNode newNode = new PathNode_Edge(cNode, cNode.navNode, neighbourLeft, false, costSoFar, 0);
                    openList.Enqueue(newNode, 1 / newNode.costSoFar);
                }
            }

            if (neighbourRight != -1 && forceDirection >= 0/* && cNode.navNode.verts[neighbourLeft].distanceBC != 0*/)
            {
                if (!IsNodeClosed(cNode.navNode, neighbourRight) && IsLineTraversable(request, cNode.StartPoint, cNode.navNode.verts[neighbourRight].PointB, out costMultiplier))
                {
                    costSoFar = CalculateDistanceFromThreats(request, cNode.navNode.verts[neighbourRight].PointB);
                    IPathNode newNode = new PathNode_Edge(cNode, cNode.navNode, neighbourRight, true, costSoFar, 0);
                    openList.Enqueue(newNode, 1 / newNode.costSoFar);
                }
            }


            if (cNode.NavVert.linkIndex.Length > 0)
            {
                int[] linkIndecies = cNode.NavVert.linkIndex;
                for (int iLink = 0; iLink < linkIndecies.Length; iLink++)
                {
                    
                    IOffNodeLink cLink = cNode.navNode.links[linkIndecies[iLink]];
                    if (forceDirection != 0)
                    {
                        if (Mathf.Sign(cLink.startPoint.x - request.start.navPoint.x) != Mathf.Sign(forceDirection))
                            continue;
                    }
                    if (!IsLinkClosed(cLink) && IsLineTraversable(request, cNode.StartPoint, cLink.startPoint, out costMultiplier))
                    {
                        costSoFar = CalculateDistanceFromThreats(request, cNode.navNode.verts[neighbourRight].PointB);
                        IPathNode newNode = new PathNode_Link(cNode, navData.nodes[cLink.targetPos.navNodeIndex], cLink.targetPos.navVertIndex, costSoFar, 0, linkIndecies[iLink]);
                        openList.Enqueue(newNode, 1 / newNode.costSoFar);
                        closedLinks.Add(cLink);
                    }
                }
            }
            forceDirection = 0;
            request.StartsInNotWalkableLight = false;
        }

        List<IPathSegment> pathSegments = new List<IPathSegment>(50);
        Vector2 prevPoint = cNode.NavVert.PointB;
        cNode = cNode.parent;
        if (cNode != null)
        {
            while (cNode.parent != null)
            {
                if (cNode.GetType() == typeof(PathNode_Edge))
                {
                    pathSegments.Add(new PathSegment(cNode.EndPoint, prevPoint, float.MaxValue));
                    prevPoint = cNode.EndPoint;
                }
                else//link
                {
                    var linkNode = (PathNode_Link)cNode;
                    pathSegments.Add(new PathSegment(linkNode.Link.targetPos.navPoint, prevPoint, float.MaxValue));
                    prevPoint = cNode.EndPoint;
                    pathSegments.Add(new JumpSegment((JumpLink)linkNode.Link));
                    prevPoint = linkNode.Link.startPoint;
                }
                cNode = cNode.parent;
            }
        }
        pathSegments.Add(new PathSegment(request.start.navPoint, prevPoint, float.MaxValue));

        IPathSegment[] inversedSeg = new IPathSegment[pathSegments.Count];
        for (int iSeg = 0, iInv = pathSegments.Count - 1; iSeg < pathSegments.Count; iSeg++, iInv--)
        {
            inversedSeg[iSeg] = pathSegments[iInv];
        }
        request.callback(new NavPath(inversedSeg));
    }

    float CalculateDistanceFromThreats(FleeRequest request, Vector2 point)
    {
        float distanceCombined = 0;
        foreach (Vector2 t in request.threats)
        {
            distanceCombined += (t - point).sqrMagnitude;
        }
        return Mathf.Sqrt(distanceCombined / request.threats.Length);
    }

    bool IsLineTraversable(ILightRequest requestData, Vector2 pointA, Vector2 pointB, out float costMultiplier)
    {
        costMultiplier = 1;
        Vector2 elevatedSegmentA = pointA + Vector2.up * navData.navAgentSettings.height;
        Vector2 elevatedSegmentB = pointB + Vector2.up * navData.navAgentSettings.height;
        int costMultiplierCount = 0;
        costMultiplier = -1;
        foreach (var marker in requestData.LightMarker)
        {
            if (marker.OverlapsSegment(pointA, pointB) || marker.OverlapsSegment(elevatedSegmentA, elevatedSegmentB))
            {
                float costMul;
                if (!marker.IsTraversable(requestData.Skin, out costMul))
                {
                    if (requestData.StartsInNotWalkableLight)
                    {
                        costMultiplier = 1;
                        return true;
                    }
                    return false;
                }
                costMultiplier += costMul;
                costMultiplierCount++;
            }
        }
        if (costMultiplier != -1)
        {
            costMultiplier /= costMultiplierCount;
            Debug.Log("Additional transition costs applied: " + costMultiplier);
        }
        return true;
    }

    bool IsNodeClosed(NavNode nn, int vertIndex)
    {
        for (int i = 0; i < closedNodes.Count; i++)
        {
            if (closedNodes[i].navNode == nn && closedNodes[i].navVertIndex == vertIndex)
                return true;
        }
        return false;
    }

    bool IsLinkClosed(IOffNodeLink link)
    {
        return closedLinks.Contains(link);
    }

    abstract class IPathNode : StablePriorityQueueNode
    {
        public IPathNode parent;

        public NavNode navNode;
        public int navVertIndex;

        public float costSoFar;
        public float totalCost;

        public abstract void GetNeighbours(out int neighbourLeft, out int neighbourRight);
        public abstract Vector2 StartPoint { get; }
        public abstract Vector2 EndPoint { get; }
        public NavNode NavNode { get { return navNode; } }
        public NavVert NavVert { get { return navNode.verts[navVertIndex]; } }

        public IPathNode(IPathNode parent, NavNode navNode, int navVertIndex, float costSoFar, float estimatedCost)
        {
            this.navNode = navNode;
            this.navVertIndex = navVertIndex;
            this.costSoFar = costSoFar;
            totalCost = estimatedCost = costSoFar;
            this.parent = parent;
        }
    }

    class PathNode_Edge : IPathNode
    {
        public bool vertIndexIsIncrementing;

        public PathNode_Edge(IPathNode parent, NavNode navNode, int navVertIndex, bool vertIndexIsIncrementing, float costSoFar, float estimatedCost) : base(parent, navNode, navVertIndex, costSoFar, estimatedCost)
        {
            this.vertIndexIsIncrementing = vertIndexIsIncrementing;
        }

        public override Vector2 EndPoint
        {
            get
            {
                if (vertIndexIsIncrementing)
                {
                    if (navVertIndex + 1 >= navNode.verts.Length)
                    {
                        Debug.Assert(navNode.isClosed);
                        return navNode.verts[0].PointB;
                    }
                    else
                    {
                        return navNode.verts[navVertIndex + 1].PointB;
                    }
                }
                else
                {
                    return NavVert.PointB;
                }
            }
        }

        public override Vector2 StartPoint
        {
            get
            {
                if (vertIndexIsIncrementing)
                {
                    return NavVert.PointB;
                }
                else
                {
                    if (navVertIndex - 1 < 0)
                    {
                        return navNode.verts[navNode.verts.Length - 1].PointB;
                    }
                    else
                    {
                        return navNode.verts[navVertIndex - 1].PointB;
                    }
                }
            }
        }

        public override void GetNeighbours(out int neighbourLeft, out int neighbourRight)
        {
            neighbourLeft = -1;
            neighbourRight = -1;
            if (vertIndexIsIncrementing)
            {
                if (navVertIndex + 1 >= navNode.verts.Length)
                {
                    if (navNode.isClosed)
                    {
                        neighbourRight = 0;
                    }
                }
                else
                {
                    neighbourRight = navVertIndex + 1;
                }
            }
            else
            {
                if (navVertIndex - 1 < 0)
                {
                    if (navNode.isClosed)
                    {
                        neighbourLeft = navNode.verts.Length - 1;
                    }
                }
                else
                {
                    neighbourLeft = navVertIndex - 1;
                }
            }
        }
    }

    class PathNode_Start : IPathNode
    {
        Vector2 start_point;

        public PathNode_Start(NavNode navNode, int navVertIndex, float estimatedCost, Vector2 startPoint) : base(null, navNode, navVertIndex, 0, estimatedCost)
        {
            this.start_point = startPoint;
        }

        public override Vector2 EndPoint
        {
            get
            {
                return start_point;
            }
        }

        public override Vector2 StartPoint
        {
            get
            {
                return start_point;
            }
        }

        public override void GetNeighbours(out int neighbourLeft, out int neighbourRight)
        {
            if (navVertIndex + 1 >= navNode.verts.Length)
            {
                if (navNode.isClosed)
                {
                    neighbourRight = 0;
                }
                else
                    neighbourRight = -1;
            }
            else
            {
                neighbourRight = navVertIndex + 1;
            }
            if (navVertIndex - 1 < 0)
            {
                if (navNode.isClosed)
                {
                    neighbourLeft = navNode.verts.Length - 1;
                }
                else
                    neighbourLeft = -1;
            }
            else
            {
                neighbourLeft = navVertIndex - 1;
            }
        }
    }

    class PathNode_Link : IPathNode
    {
        int linkIndex;

        public PathNode_Link(IPathNode parent, NavNode navNode, int navVertIndex, float costSoFar, float estimatedCost, int linkIndex) : base(parent, navNode, navVertIndex, costSoFar, estimatedCost)
        {
            this.linkIndex = linkIndex;
        }

        public override Vector2 StartPoint
        {
            get
            {
                return Link.startPoint;
            }
        }

        public IOffNodeLink Link { get { return parent.navNode.links[linkIndex]; } }

        public override Vector2 EndPoint
        {
            get
            {
                return Link.targetPos.navPoint;
            }
        }

        public override void GetNeighbours(out int neighbourLeft, out int neighbourRight)
        {
            var endNode = NavNode;
            if (Link.targetPos.navVertIndex + 1 >= endNode.verts.Length)
            {
                if (endNode.isClosed)
                {
                    neighbourRight = 0;
                }
                else
                    neighbourRight = -1;
            }
            else
            {
                neighbourRight = Link.targetPos.navVertIndex + 1;
            }
            if (Link.targetPos.navVertIndex - 1 < 0)
            {
                if (endNode.isClosed)
                {
                    neighbourLeft = endNode.verts.Length - 1;
                }
                else
                    neighbourLeft = -1;
            }
            else
            {
                neighbourLeft = Link.targetPos.navVertIndex - 1;
            }
        }
    }
}
/*
public class PathRequest : ILightRequest
{
    public delegate void PathCompleted(NavPath path);

    public readonly NavPosition start;
    public readonly NavPosition goal;
    readonly LightSkin skin;
    readonly LightMarker[] lightMarker;
    bool startsInNotWalkableLight;
    public readonly PathCompleted callback;

    public LightSkin Skin
    {
        get
        {
            return skin;
        }
    }

    public LightMarker[] LightMarker
    {
        get
        {
            return lightMarker;
        }
    }

    public bool StartsInNotWalkableLight
    {
        get
        {
            return startsInNotWalkableLight;
        }
        set
        {
            startsInNotWalkableLight = value;
        }
    }


    public PathRequest(NavPosition start, NavPosition goal, PathCompleted callback, LightSkin skin, bool startsInNotWalkableLight, params LightMarker[] lightMarker)
    {
        this.skin = skin;
        this.start = start;
        this.goal = goal;
        this.callback = callback;
        this.lightMarker = lightMarker;
        this.startsInNotWalkableLight = startsInNotWalkableLight;
    }
}

public class FleeRequest : ILightRequest
{
    public delegate void PathCompleted(NavPath path);

    public readonly NavPosition start;
    public readonly Vector2[] threats;
    LightSkin skin;
    readonly LightMarker[] lightMarker;
    bool startsInNotWalkableLight;
    public readonly PathCompleted callback;
    public readonly float targetDistanceFromThreats;

    public LightSkin Skin
    {
        get
        {
            return skin;
        }
    }

    public LightMarker[] LightMarker
    {
        get
        {
            return lightMarker;
        }
    }

    public bool StartsInNotWalkableLight
    {
        get
        {
            return startsInNotWalkableLight;
        }
        set
        {
            startsInNotWalkableLight = value;
        }
    }

    public FleeRequest(NavPosition start, Vector2[] threats, PathCompleted callback, LightSkin skin, bool startsInNotWalkableLight, float targetDistanceFromThreats, params LightMarker[] lightMarker)
    {
        this.skin = skin;
        this.start = start;
        this.threats = threats;
        this.callback = callback;
        this.lightMarker = lightMarker;
        this.startsInNotWalkableLight = startsInNotWalkableLight;
        this.targetDistanceFromThreats = targetDistanceFromThreats;
    }
}

interface ILightRequest
{
    LightSkin Skin { get; }
    LightMarker[] LightMarker { get; }
    bool StartsInNotWalkableLight { get; set; }
}
#endif
}
    [Serializable]
    public class NavPath
    {
        public IPathSegment[] pathSegments;

        public NavPath(IPathSegment[] pathSegments)
        {
            this.pathSegments = pathSegments;
        }

        public void Visualize()
        {
            foreach (IPathSegment seg in pathSegments)
            {
                //seg.Visualize();
            }
        }
    }

