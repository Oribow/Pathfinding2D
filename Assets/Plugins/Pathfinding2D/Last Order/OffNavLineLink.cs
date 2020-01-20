using UnityEngine;
using System.Collections;
using System;
using NavData2d.Editor;


public class OffNavLineLink : MonoBehaviour
{
    public NavPosition2d NavPosStart { get { return navPosStart; } }
    public NavPosition2d NavPosEnd { get { return navPosEnd; } }

    [SerializeField]
    private Transform start;
    [SerializeField]
    private Transform end;
    [SerializeField]
    private float costOverride = 1;
    [SerializeField]
    private bool biDirectional = true;

    [SerializeField, ReadOnly]
    private NavPosition2d navPosStart;
    [SerializeField, ReadOnly]
    private NavPosition2d navPosEnd;

    private void Start()
    {
        var surface = GetComponentInParent<NavSurface2d>();
        if (surface != null) { 
            
        }
    }

    public void UpdateLink()
    {
        var surface = GetComponentInParent<NavSurface2d>();
        if (surface == null)
        {
            Debug.LogError("Couldn't find a NavSurface2d component on parents");
            return;
        }

        surface.NearestNavPosition2d(start.position, out navPosStart);
        surface.NearestNavPosition2d(end.position, out navPosEnd);
    }

    void OnDrawGizmos()
    {
        if (start != null)
            DrawLinkPosition(navPosStart, start.position);
        if (end != null)
            DrawLinkPosition(navPosEnd, end.position);

        if (end != null && start != null)
        {
            Vector2 start = navPosStart == null ? (Vector2)this.start.position : navPosStart.Point;
            Vector2 end = navPosEnd == null ? (Vector2)this.end.position : navPosEnd.Point;

            // draw connection
            float x = start.x + (end.x - start.x) * 0.5f;
            float y = Mathf.Max(start.y, end.y) + 1;
            Vector2 cp = new Vector2(x, y);

            Vector2 prev = start;
            for (int t = 1; t <= 10; t ++)
            {
                Vector2 v = ABC.Utility.QuadraticBezierCurve(t / 10f, start, cp, end);
                Gizmos.DrawLine(prev, v);
                prev = v;
            }

            //draw arrows
            Vector2 p = ABC.Utility.QuadraticBezierCurve(0.9f, start, cp, end);
            ABC.Utility.DrawArrow(p, end - p);
            if (biDirectional)
            {
                p = ABC.Utility.QuadraticBezierCurve(0.1f, start, cp, end);
                ABC.Utility.DrawArrow(p, start - p);
            }
        }
    }

    private void DrawLinkPosition(NavPosition2d navPos, Vector2 orgPos)
    {
        Gizmos.DrawCube(orgPos, Vector3.one * 0.1f);
        if (navPos != null)
        {
            Vector2 segmentTangent = navPos.SegmentTangent.normalized * 0.2f;
            Gizmos.DrawLine(navPos.Point - segmentTangent, navPos.Point + segmentTangent);

            Vector2 segmentNormal = navPos.SegmentNormal.normalized * 0.1f;
            Gizmos.DrawLine(navPos.Point - segmentTangent + segmentNormal, navPos.Point + segmentTangent + segmentNormal);
        }
    }
}
