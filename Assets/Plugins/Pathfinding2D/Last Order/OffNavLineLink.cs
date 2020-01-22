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
        UpdateLink();
        if (navPosStart != null) {
            navPosStart.InsertIntoGraph();
        }
        if (navPosEnd != null)
        {
            navPosEnd.InsertIntoGraph();
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

        if (!surface.FindNavPosition2d(start.position, out navPosStart))
        {
            Debug.LogError("Couldn't find start nav position.");
        }
        if (!surface.FindNavPosition2d(end.position, out navPosEnd))
        {
            Debug.LogError("Couldn't find end nav position.");
        }
    }

    void OnDrawGizmos()
    {
        if (start != null)
            DrawLinkPosition(navPosStart, start.position);
        if (end != null)
            DrawLinkPosition(navPosEnd, end.position);

        if (end != null && start != null)
        {
            Vector2 start = navPosStart == null ? (Vector2)this.start.position : navPosStart.Position;
            Vector2 end = navPosEnd == null ? (Vector2)this.end.position : navPosEnd.Position;

            // draw connection
            ABC.Utility.DrawBezierConnection(start, end, biDirectional);
        }
    }

    private void DrawLinkPosition(NavPosition2d navPos, Vector2 orgPos)
    {
        Gizmos.DrawCube(orgPos, Vector3.one * 0.1f);
        if (navPos != null)
        {
            Vector2 segmentTangent = navPos.Tangent * 0.2f;
            Gizmos.DrawLine(navPos.Position - segmentTangent, navPos.Position + segmentTangent);

            Vector2 segmentNormal = navPos.Normal * 0.1f;
            Gizmos.DrawLine(navPos.Position - segmentTangent + segmentNormal, navPos.Position + segmentTangent + segmentNormal);
        }
    }
}
