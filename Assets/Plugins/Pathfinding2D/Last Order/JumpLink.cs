using UnityEngine;
using System.Collections;
using System;
using NavData2d.Editor;


public class JumpLink : MonoBehaviour
{
    [SerializeField]
    private Vector2 start = Vector2.zero;
    [SerializeField]
    private Vector2 end = Vector2.right;
    [SerializeField]
    private NavAgentType navAgentType;
    [SerializeField]
    private float costMultiplier = 1;
    [SerializeField]
    private bool isBiDirectional = true;
    [SerializeField, Range(0, 1)]
    private float jumpForceBias = 0;

    [SerializeField]
    private bool wrapToGround = true;
    [SerializeField]
    private LayerMask groundLayers = int.MaxValue;

    private Vector2 _start;
    private Vector2 _end;
    private float xVelocity;
    private float maxJumpForce;
    private float minJumpForce;
    private bool isValid;

    public void UpdateLink()
    {
        if (wrapToGround)
            WarpPointsToGround();
        isValid = navAgentType.CalculateJumpArc(_start, _end, out xVelocity, out maxJumpForce);
    }

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
    }

    void OnDrawGizmos()
    {
        if (navAgentType == null)
            return;

        Gizmos.color = Color.white;

        Gizmos.DrawLine(_start, _end);
        Vector2 tangent = (_end - _start);
        float dist = tangent.magnitude;
        tangent /= dist;
        Vector2 arrowOrigin = (tangent * (dist / 2)) + _start;


        if (!isBiDirectional)
        {
            Gizmos.DrawLine(arrowOrigin, (Quaternion.Euler(0, 0, 30) * -tangent * 0.2f) + (Vector3)arrowOrigin);
            Gizmos.DrawLine(arrowOrigin, (Quaternion.Euler(0, 0, -30) * -tangent * 0.2f) + (Vector3)arrowOrigin);
            arrowOrigin += tangent * 0.2f;
            Gizmos.DrawLine(arrowOrigin, (Quaternion.Euler(0, 0, 30) * -tangent * 0.2f) + (Vector3)arrowOrigin);
            Gizmos.DrawLine(arrowOrigin, (Quaternion.Euler(0, 0, -30) * -tangent * 0.2f) + (Vector3)arrowOrigin);
            arrowOrigin -= tangent * 0.4f;
            Gizmos.DrawLine(arrowOrigin, (Quaternion.Euler(0, 0, 30) * -tangent * 0.2f) + (Vector3)arrowOrigin);
            Gizmos.DrawLine(arrowOrigin, (Quaternion.Euler(0, 0, -30) * -tangent * 0.2f) + (Vector3)arrowOrigin);

            arrowOrigin += tangent * 0.2f;
        }
        else
        {
            arrowOrigin += tangent * 0.1f;
            Gizmos.DrawLine(arrowOrigin, (Quaternion.Euler(0, 0, 30) * tangent * 0.2f) + (Vector3)arrowOrigin);
            Gizmos.DrawLine(arrowOrigin, (Quaternion.Euler(0, 0, -30) * tangent * 0.2f) + (Vector3)arrowOrigin);
            arrowOrigin += tangent * 0.2f;
            Gizmos.DrawLine(arrowOrigin, (Quaternion.Euler(0, 0, 30) * tangent * 0.2f) + (Vector3)arrowOrigin);
            Gizmos.DrawLine(arrowOrigin, (Quaternion.Euler(0, 0, -30) * tangent * 0.2f) + (Vector3)arrowOrigin);
            arrowOrigin -= tangent * 0.3f;
            Gizmos.DrawLine(arrowOrigin, (Quaternion.Euler(0, 0, 30) * -tangent * 0.2f) + (Vector3)arrowOrigin);
            Gizmos.DrawLine(arrowOrigin, (Quaternion.Euler(0, 0, -30) * -tangent * 0.2f) + (Vector3)arrowOrigin);
            arrowOrigin -= tangent * 0.2f;
            Gizmos.DrawLine(arrowOrigin, (Quaternion.Euler(0, 0, 30) * -tangent * 0.2f) + (Vector3)arrowOrigin);
            Gizmos.DrawLine(arrowOrigin, (Quaternion.Euler(0, 0, -30) * -tangent * 0.2f) + (Vector3)arrowOrigin);

            arrowOrigin += tangent * 0.2f;
        }

        Gizmos.DrawLine(_start, start);
        Gizmos.DrawLine(_end, end);


        Vector2 upRight, downRight, upLeft, downLeft;
        float halfWidth = navAgentType.width / 2;
        upRight = new Vector2(_start.x - halfWidth, _start.y + navAgentType.height);
        upLeft = new Vector2(_start.x + halfWidth, _start.y + navAgentType.height);
        downLeft = new Vector2(_start.x + halfWidth, _start.y);
        downRight = new Vector2(_start.x - halfWidth, _start.y);
        Gizmos.DrawLine(upRight, upLeft);
        Gizmos.DrawLine(upLeft, downLeft);
        Gizmos.DrawLine(downLeft, downRight);
        Gizmos.DrawLine(downRight, upRight);

        Vector2 endPointOffset = _end - _start;
        Gizmos.DrawLine(upRight + endPointOffset, upLeft + endPointOffset);
        Gizmos.DrawLine(upLeft + endPointOffset, downLeft + endPointOffset);
        Gizmos.DrawLine(downLeft + endPointOffset, downRight + endPointOffset);
        Gizmos.DrawLine(downRight + endPointOffset, upRight + endPointOffset);

        /*
        Gizmos.DrawWireDisc(navPosA.navPoint, Vector3.forward, 0.05f);
        Gizmos.DrawWireDisc(navPosB.navPoint, Vector3.forward, 0.05f);

        Gizmos.DrawWireDisc(downRight, Vector3.forward, 0.1f);
        Gizmos.DrawWireDisc(downLeft, Vector3.forward, 0.1f);

        Gizmos.DrawWireDisc(downLeft + endPointOffset, Vector3.forward, 0.1f);
        Gizmos.DrawWireDisc(downRight + endPointOffset, Vector3.forward, 0.1f);
        */
        if (isValid)
        {
            Gizmos.color = Color.green;

            if (_start.x > _end.x)
            {
                upRight.x -= _start.x;
                upLeft.x -= _start.x;
                downLeft.x -= _start.x;
                downRight.x -= _start.x;
                DrawJumpArc(upRight);
                DrawJumpArc(upLeft);
                DrawJumpArc(downLeft);
                DrawJumpArc(downRight);
            }
            else
            {
                upRight.x -= _start.x;
                upLeft.x -= _start.x;
                downLeft.x -= _start.x;
                downRight.x -= _start.x;
                DrawJumpArc(upRight);
                DrawJumpArc(upLeft);
                DrawJumpArc(downLeft);
                DrawJumpArc(downRight);
            }
        }
    }

    void DrawJumpArc(Vector2 offset)
    {
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
        }
        //Handles.DrawLine(prevPos, new Vector2(link.jumpArc.endX, link.jumpArc.Calc(link.jumpArc.maxX - link.jumpArc.minX)) + origin);
    }
}
