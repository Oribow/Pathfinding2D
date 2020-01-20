﻿using UnityEngine;
using System.Linq;
using UnityEditor;
using System.Collections.Generic;
using NavGraph.Build;

[RequireComponent(typeof(RectTransform))]
public class NavSurface2d : MonoBehaviour
{
    const float pointNavLineMaxDistance = 3;

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

    private PolygonSet polygonSet;
    [SerializeField, ReadOnly]
    private List<NavLine> navLines;

    public void Bake()
    {
        polygonSet = CollectNavigationPolygons();
        var it = new IntersectionTester();
        if (navAgentTypes == null || navAgentTypes.Length > 0)
        {
            navLines = it.Mark(polygonSet, navAgentTypes[0]);

            // convert coordinates back
            foreach (var line in navLines)
            {
                foreach (var segment in line.segments)
                {
                    segment.start /= floatToIntMult;
                }
            }

            Debug.Log("Created " + navLines.Count + " navlines");
        }
        else
        {
            Debug.LogError("No agent type specified");
        }
    }

    public bool NearestNavPosition2d(Vector2 point, out NavPosition2d navPosition)
    {
        float minDistance = float.MaxValue;
        NavLine closestNavLine = null;
        int closestNavLineSegmentIndex = -1;
        Vector2 closestPoint = Vector2.zero;

        foreach (var line in navLines)
        {
            int segIndex;
            Vector2 p;
            float dist = line.DistanceToPoint(point, out segIndex, out p);

            if (dist < minDistance)
            {
                minDistance = dist;
                closestNavLine = line;
                closestNavLineSegmentIndex = segIndex;
                closestPoint = p;
            }
        }

        if (minDistance == float.MaxValue)
        {
            navPosition = null;
            return false;
        }
        else
        {

            navPosition = new NavPosition2d(closestNavLine, closestNavLineSegmentIndex, closestPoint);
            return true;
        }
    }

    public void AddLink(OffNavLineLink link)
    { 
        
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
            Gizmos.color = Color.green;
            foreach (var line in navLines)
            {
                for (int iSeg = 0; iSeg < line.segments.Length - 1; iSeg++)
                {
                    Gizmos.DrawLine(line.segments[iSeg].start, line.segments[iSeg + 1].start);
                }

                // draw special beginning and end marker
                if (line.isClosed)
                {
                    Gizmos.DrawLine(line.segments[0].start, line.segments[line.segments.Length - 1].start);
                }
                else
                {
                    if (line.segments.Length >= 2)
                    {
                        var start = line.segments[0].start;
                        var start2 = line.segments[1].start;
                        var normal = (start2 - start);
                        normal = new Vector2(-normal.y, normal.x).normalized;

                        Gizmos.DrawLine(start + (normal * -0.2f), start + (normal * 0.2f));

                        var end = line.segments[line.segments.Length - 1].start;
                        var end2 = line.segments[line.segments.Length - 2].start;

                        normal = (end2 - end);
                        normal = new Vector2(-normal.y, normal.x).normalized;

                        Gizmos.DrawLine(end + (normal * -0.2f), end + (normal * 0.2f));
                    }
                }
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
    }
}
