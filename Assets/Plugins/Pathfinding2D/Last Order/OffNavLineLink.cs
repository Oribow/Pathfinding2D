using UnityEngine;
using System.Collections;
using System;
using NavData2d.Editor;

public interface INavLink {
    bool IsBiDirectional { get; }
    NavPosition2d StartPosition { get; }
    NavPosition2d EndPosition { get; }
}


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
    private LinkPoint linkStart;
    [SerializeField, ReadOnly]
    private LinkPoint linkEnd;
    /*
    public LinkPoint LinkStart { get { return linkStart; } }
    public LinkPoint LinkEnd { get { return linkEnd; } }

    public void UpdateLink()
    {
        if (linkStart != null && linkStart.navSurface != null)
        {
            linkStart.navSurface.RemoveLinkStartPoint(linkStart);
        }
        if (linkEnd != null && linkEnd.navSurface != null)
        {
            linkEnd.navSurface.RemoveLinkEndPoint(linkStart);
        }

        var surface = GetComponentInParent<NavSurface2d>();
        if (surface == null)
        {
            Debug.LogError("Couldn't find a NavSurface2d component on parents");
            return;
        }
        NavPosition2d navPosStart, navPosEnd;
        if (!surface.FindNavPosition2d(start.position, out navPosStart))
        {
            Debug.LogError("Couldn't find start nav position.");
        }
        if (!surface.FindNavPosition2d(end.position, out navPosEnd))
        {
            Debug.LogError("Couldn't find end nav position.");
        }
        if (navPosStart != null && navPosEnd != null)
        {
            linkStart = new LinkPoint(navPosStart, this, true);
            linkEnd = new LinkPoint(navPosEnd, this, false);

            surface.AddLinkStartPoint(linkStart);
            surface.AddLinkEndPoint(linkEnd);

            if (biDirectional)
            {
                surface.AddLinkStartPoint(linkEnd);
                surface.AddLinkEndPoint(linkStart);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (start != null)
            DrawLinkPosition(linkStart, start.position);
        if (end != null)
            DrawLinkPosition(linkEnd, end.position);

        if (end != null && start != null)
        {
            Vector2 start = linkStart == null ? (Vector2)this.start.position : linkStart.Position;
            Vector2 end = linkEnd == null ? (Vector2)this.end.position : linkEnd.Position;

            // draw connection
            ABC.Utility.DrawBezierConnection(start, end, biDirectional);
        }
    }

    private void DrawLinkPosition(LinkPoint linkP, Vector2 orgPos)
    {
        Gizmos.DrawCube(orgPos, Vector3.one * 0.1f);
        if (linkP != null)
        {
            Vector2 segmentTangent = linkP.Segment.Tangent * 0.2f;
            Gizmos.DrawLine(linkP.Position - segmentTangent, linkP.Position + segmentTangent);

            Vector2 segmentNormal = linkP.Segment.Normal * 0.1f;
            Gizmos.DrawLine(linkP.Position - segmentTangent + segmentNormal, linkP.Position + segmentTangent + segmentNormal);
        }
    }*/
}

/*
[Serializable]
public class LinkPoint : NavPosition2d {
    public NavSegment Segment { get { return navSurface.GetSegment(LineIndex, SegmentIndex); } }
    public OffNavLineLink Link { get { return link; } }
    public LinkPoint OtherPoint { get { return isStart ? link.LinkEnd : link.LinkStart; } }

    [System.NonSerialized]
    public int navNodeIndex;

    [SerializeField]
    OffNavLineLink link;
    [SerializeField]
    public NavSurface2d navSurface;
    [SerializeField]
    bool isStart;

    public LinkPoint(Vector2 position, int lineIndex, int segmentIndex, OffNavLineLink link, bool isStart) : base(position, lineIndex, segmentIndex)
    {
        this.link = link;
        this.isStart = isStart;
    }

    public LinkPoint( NavPosition2d navPos,OffNavLineLink link, bool isStart): base(navPos.Position, navPos.LineIndex, navPos.SegmentIndex)
    {
        this.link = link;
        this.isStart = isStart;
    }

    public void Invalidate() {
        this.navSurface = null;
        navNodeIndex = -1;
    }

    public bool IsValid()
    {
        return this.navSurface != null;
    }
}
*/