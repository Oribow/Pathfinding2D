using UnityEngine;
using System.Linq;
using UnityEditor;
using System.Collections.Generic;
using NavGraph.Build;
using System;

[RequireComponent(typeof(RectTransform))]
public class NavSurface2d : MonoBehaviour//, INavNodeConstructor
{
    /*
    const float pointNavLineMaxDistance = 3;

    public static void AddLink(INavLink link)
    {
        LinkPoint startP = new LinkPoint(link.IsBiDirectional);
        LinkPoint endP = new LinkPoint(true);

        startP.other = endP;
        endP.other = startP;


    }

    private enum CollectObjectsMethod
    {
        All,
        Volume,
        Children
    }
    [Header("Agents")]
    [SerializeField]
    private NavAgentType[] navAgentTypes;

    [Header("Filter")]
    [SerializeField]
    private CollectObjectsMethod collectObjects = CollectObjectsMethod.All;
    [SerializeField]
    private LayerMask includeLayers = int.MaxValue;

    [Header("Conversion")]
    [SerializeField, Range(4, 100)]
    int circleVertCount = 20;

    [Header("DEBUG/TMP")]
    [SerializeField]
    int floatToIntMult = 1000;
    [SerializeField]
    bool drawUnionPolygons;
    [SerializeField]
    bool drawNavSegments;
    [SerializeField]
    bool drawNavNodes;

    private PolygonSet polygonSet;
    [SerializeField, ReadOnly]
    private List<NavLine> navLines;

    [SerializeField, HideInInspector]
    private List<LinkPoint> links;

    public List<NavLine> NavLines { get { return navLines; } }

    public event EventHandler SurfaceBakeEvent;

    private void Start()
    {
        AddToNavGraph();
    }

    public NavSegment GetSegment(int lineIndex, int segmentIndex)
    {
        return this.navLines[lineIndex].segments[segmentIndex];
    }

    public void Bake()
    {
        polygonSet = CollectNavigationPolygons();
        var it = new IntersectionTester();
        if (navAgentTypes == null || navAgentTypes.Length > 0)
        {
            navLines = it.Mark(polygonSet, navAgentTypes[0]);
            Debug.Log("Created " + navLines.Count + " navlines");

            foreach (var l in linkEPs)
            {
                l.Invalidate();
            }
            linkEPs.Clear();
            SurfaceBakeEvent?.Invoke(this, null);
        }
        else
        {
            Debug.LogError("No agent type specified");
        }
    }

    public bool FindNavPosition2d(Vector2 point, out NavPosition2d navPosition)
    {
        float minDistance = float.MaxValue;
        int closestSegment = -1;
        int closestLine = -1;
        Vector2 closestPoint = Vector2.zero;

        int lineIndex = 0;
        foreach (var line in navLines)
        {
            int seg;
            Vector2 p;
            float dist = line.DistanceToPoint(point, out seg, out p);

            if (dist < minDistance)
            {
                minDistance = dist;
                closestSegment = seg;
                closestPoint = p;
                closestLine = lineIndex;
            }
            lineIndex++;
        }

        if (minDistance == float.MaxValue)
        {
            navPosition = null;
            return false;
        }
        else
        {
            navPosition = new NavPosition2d(closestPoint, closestLine, closestSegment);
            return true;
        }
    }

    public void AddLink(LinkPoint link)
    {
        this.links.Add(link);
    }

    public void RemoveLinkEndPoint(LinkPoint linkEP)
    {
        linkEPs.Remove(linkEP);
        linkEP.navSurface = null;
    }

    private void AddToNavGraph()
    {
        var graph = NavGraph2d.Instance;
        foreach (var line in navLines)
        {
            var segments = line.segments;
            // create nav nodes for segments
            foreach (var segment in segments)
            {
                var node = new NavNode2d(segment.Start, false);
                segment.navNodeIndex = graph.AddNode(node);
            }

            int startIndex;
            NavSegment prevSeg;
            if (line.IsClosed)
            {
                startIndex = 0;
                prevSeg = segments[line.segments.Length - 1];

                var prevPrevSeg = segments[segments.Length - 2];
                var node = graph.GetNode(prevSeg.navNodeIndex);
                node.Prev = new NavNodeConnection(prevPrevSeg.navNodeIndex, prevPrevSeg.TotalTraversalCost);
                node.Next = new NavNodeConnection(segments[0].navNodeIndex, prevSeg.TotalTraversalCost);
            }
            else
            {
                startIndex = 1;
                prevSeg = segments[0];

                NavSegment lastSegment = segments[segments.Length - 1];
                NavNode2d prevLastNode = graph.GetNode(lastSegment.navNodeIndex);
                NavNode2d lastNode = new NavNode2d(lastSegment.End, false);
                line.lastNavNodeIndex = graph.AddNode(lastNode);

                lastNode.Prev = new NavNodeConnection(lastSegment.navNodeIndex, lastSegment.TotalTraversalCost);
                prevLastNode.Next = new NavNodeConnection(line.lastNavNodeIndex, lastSegment.TotalTraversalCost);

                if (segments.Length >= 2)
                    graph.GetNode(prevSeg.navNodeIndex).Next = new NavNodeConnection(segments[1].navNodeIndex, segments[0].TotalTraversalCost);
            }

            for (int iSeg = startIndex; iSeg < line.segments.Length - 1; iSeg++)
            {
                var seg = line.segments[iSeg];
                var nextSeg = line.segments[iSeg + 1];

                var node = graph.GetNode(seg.navNodeIndex);
                node.Prev = new NavNodeConnection(prevSeg.navNodeIndex, prevSeg.TotalTraversalCost);
                node.Next = new NavNodeConnection(nextSeg.navNodeIndex, seg.TotalTraversalCost);

                prevSeg = seg;
            }
        }

        foreach (var linkEP in linkEPs)
        {
            linkEP.navNodeIndex = NavGraph2d.Instance.AddLinkNode(linkEP.Position, linkEP.Segment);

        }

        foreach (var linkSP in linkSPs)
        {
            linkSP.navNodeIndex = NavGraph2d.Instance.AddLinkNode(linkSP.Position, linkSP.Segment);
        }
    }

    private void RemoveFromNavGraph()
    {
        var graph = NavGraph2d.Instance;
        // remove nav nodes and all connections to em
        // 1. find all nav nodes this surface spawned and delete them
        foreach (var line in navLines)
        {
            foreach (var seg in line.segments)
            {
                graph.RemoveNode(seg.navNodeIndex);
            }
            if (line.lastNavNodeIndex != 0)
                graph.RemoveNode(line.lastNavNodeIndex);
        }

        // 2. go through all connections to the surface and delete them
        foreach (var linkEp in linkEPs)
        {
            linkEp.Invalidate();
        }
    }

    private PolygonSet CollectNavigationPolygons()
    {
        // 1. Collect collider
        IEnumerable<Collider2D> navObjects;
        switch (collectObjects)
        {
            case CollectObjectsMethod.All:
                var allCollider = GameObject.FindObjectsOfType<Collider2D>();
                navObjects = (from item in allCollider
                              where
                              GameObjectUtility.AreStaticEditorFlagsSet(item.gameObject, StaticEditorFlags.NavigationStatic) &&
                              ((1 << item.gameObject.layer) & includeLayers) != 0
                              select item
                 );

                break;
            case CollectObjectsMethod.Volume:
                var rectTransform = GetComponent<RectTransform>();
                var overlapedColliders = Physics2D.OverlapBoxAll(rectTransform.position, rectTransform.rect.size, rectTransform.eulerAngles.z, includeLayers);
                navObjects = (from item in overlapedColliders
                              where GameObjectUtility.AreStaticEditorFlagsSet(item.gameObject, StaticEditorFlags.NavigationStatic)
                              select item);
                break;
            case CollectObjectsMethod.Children:
                var childCollider = this.GetComponentsInChildren<Collider2D>();
                navObjects = (from item in childCollider
                              where
                              GameObjectUtility.AreStaticEditorFlagsSet(item.gameObject, StaticEditorFlags.NavigationStatic) &&
                              (item.gameObject.layer & includeLayers) > 0
                              select item
                 );
                break;
            default:
                navObjects = Enumerable.Empty<Collider2D>();
                Debug.LogError("Unkown collections method: " + collectObjects);
                break;
        }

        // 2. convert collider to geometry
        PolygonSet polygonSet = new PolygonSet(circleVertCount, floatToIntMult);
        int count = 0;
        foreach (var col in navObjects)
        {
            polygonSet.AddCollider(col);
            count++;
        }
        Debug.Log("Processed " + count + " collider, resulting in " + polygonSet.Polygons.Count + " polygons.");
        return polygonSet;
    }

    private void OnDrawGizmos()
    {
        if (navLines != null)
        {
            try
            {
                if (drawNavSegments)
                {
                    Gizmos.color = Color.green;
                    foreach (var line in navLines)
                    {
                        for (int iSeg = 0; iSeg < line.segments.Length; iSeg++)
                        {
                            Gizmos.DrawLine(line.segments[iSeg].Start, line.segments[iSeg].End);
                        }

                        // draw special beginning and end marker
                        if (line.segments.Length >= 2 && !line.IsClosed)
                        {
                            var start = line.segments[0].Start;
                            var start2 = line.segments[1].Start;
                            var normal = (start2 - start);
                            normal = new Vector2(-normal.y, normal.x).normalized;

                            Gizmos.DrawLine(start + (normal * -0.2f), start + (normal * 0.2f));

                            var end = line.segments[line.segments.Length - 1].Start;
                            var end2 = line.segments[line.segments.Length - 2].Start;

                            normal = (end2 - end);
                            normal = new Vector2(-normal.y, normal.x).normalized;

                            Gizmos.DrawLine(end + (normal * -0.2f), end + (normal * 0.2f));
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                navLines = null;
            }
        }


        if (polygonSet != null && drawUnionPolygons)
        {
            foreach (var poly in polygonSet.Polygons)
            {
                Gizmos.color = Color.red;
                DrawContour(poly.hull);

                Gizmos.color = Color.blue;
                foreach (var hole in poly.holes)
                {
                    DrawContour(hole);
                }
            }
        }
    }

    private void DrawContour(Contour contour)
    {
        Vector2 prevPoint = polygonSet.IntPointToVector2(contour.Verts[contour.VertexCount - 1]);
        foreach (var point in contour.Verts)
        {
            var v = polygonSet.IntPointToVector2(point);
            DebugExtension.DrawArrow(prevPoint, v - prevPoint);

            prevPoint = v;
        }
    }*/
}

class LinkPoint {
    public int navNodeIndex;
    public LinkPoint other;
    public bool canBeGoal;

    public LinkPoint(bool canBeGoal)
    {
        this.canBeGoal = canBeGoal;
    }
}