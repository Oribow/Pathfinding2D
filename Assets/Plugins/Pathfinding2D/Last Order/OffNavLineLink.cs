using UnityEngine;
using System.Collections;
using System;
using NavData2d.Editor;


public class OffNavLineLink : MonoBehaviour
{
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
    /*
    void WarpPointsToGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(start, Vector2.down, 10, groundLayers);
        if (hit.collider != null)
        {
            _start = hit.point;
        }
        hit = Physics2D.Raycast(end, Vector2.down, 10, groundLayers);
        if (hit.collider != null)
        {
            _end = hit.point;
        }
    }*/

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


    void DrawJumpArc(Vector2 offset)
    {
        /*
        Vector2 swapPos;
        Vector2 prevPos = _start + offset;
        float absStepWidth = (_end.x - _start.x) / 100f;
        float stepWidth = absStepWidth * (_end.x < _start.x ? -1f : 1f);

        for (int n = 0; n <= 100; n++)
        {
            float x = n * stepWidth;
            float y = x / xVelocity;
            y = (maxJumpForce - navAgentType.gravity / 2f * x) * x;

            swapPos = new Vector2(_start.x + x, y) + offset;
            Gizmos.DrawLine(prevPos, swapPos);
            prevPos = swapPos;
        }*/
        //Handles.DrawLine(prevPos, new Vector2(link.jumpArc.endX, link.jumpArc.Calc(link.jumpArc.maxX - link.jumpArc.minX)) + origin);
    }
}
