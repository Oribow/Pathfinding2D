using Pathfinding2d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace NavData2d.Editor
{
    [System.Serializable]
    public class MetaJumpLink
    {
        [SerializeField]
        public Vector2 worldPointA;
        [SerializeField]
        public Vector2 worldPointB;

        [SerializeField, Range(0, 1)]
        public float xSpeedScale = 1;

        [SerializeField, ReadOnly]
        public NavPosition navPosA;
        [SerializeField, ReadOnly]
        public NavPosition navPosB;

        [SerializeField]
        public JumpArcSegment jumpArc;

        [SerializeField]
        public bool isJumpLinkValid;

        [SerializeField]
        public bool isBiDirectional;

        [SerializeField]
        public JumpLinkSettings jumpLinkSettings;

        public MetaJumpLink()
        {
            jumpLinkSettings = new JumpLinkSettings();
        }

        public MetaJumpLink(Vector2 worldPointA, Vector2 worldPointB)
        {
            this.worldPointA = worldPointA;
            this.worldPointB = worldPointB;
            jumpLinkSettings = new JumpLinkSettings();
        }

        public MetaJumpLink InvertLink(NavAgentGroundWalkerSettings groundWalkerSettings)
        {
            MetaJumpLink inverseLink = new MetaJumpLink();
            inverseLink.worldPointA = this.worldPointB;
            inverseLink.worldPointB = this.worldPointA;
            inverseLink.xSpeedScale = this.xSpeedScale;
            inverseLink.isBiDirectional = this.isBiDirectional;
            inverseLink.navPosA = this.navPosB;
            inverseLink.navPosB = this.navPosA;
            inverseLink.RecalculateJumpArc(groundWalkerSettings);
            return inverseLink;
        }

        public bool TryRemapPoints(NavigationData2D navData)
        {
            if (!navData.SamplePoint(worldPointA, out navPosA))
            {
                return false;
            }

            if (!navData.SamplePoint(worldPointB, out navPosB))
            {
                return false;
            }
            return true;
        }

        public void RecalculateJumpArc(NavAgentGroundWalkerSettings groundWalkerSettings)
        {
            float t = Mathf.Abs(navPosB.navPoint.x - navPosA.navPoint.x) / (groundWalkerSettings.maxXVel * xSpeedScale);
            float arcTargetJ = (groundWalkerSettings.gravity * t * 0.5f) + ((navPosB.navPoint.y - navPosA.navPoint.y) / t);

            if (Mathf.Abs(arcTargetJ) > groundWalkerSettings.jumpForce || arcTargetJ < 0)
                isJumpLinkValid = false;
            else
                isJumpLinkValid = true;
            if (jumpArc == null)
                jumpArc = new JumpArcSegment(arcTargetJ, groundWalkerSettings.gravity, groundWalkerSettings.maxXVel * xSpeedScale, navPosA.navPoint.x, navPosB.navPoint.x);
            else
                jumpArc.UpdateArc(arcTargetJ, groundWalkerSettings.gravity, groundWalkerSettings.maxXVel * xSpeedScale, navPosA.navPoint.x, navPosB.navPoint.x);
        }

        public void DrawJumpLink(float agentWidth, float agentHeight, bool showInSceneDetailed, bool isBiDirectional, bool editable)
        {
            Handles.color = Color.white;

            Handles.DrawLine(navPosA.navPoint, navPosB.navPoint);
            Vector2 tangent = (navPosB.navPoint - navPosA.navPoint);
            float dist = tangent.magnitude;
            tangent /= dist;
            Vector2 arrowOrigin = (tangent * (dist / 2)) + navPosA.navPoint;


            if (!isBiDirectional)
            {
                Handles.DrawLine(arrowOrigin, (Quaternion.Euler(0, 0, 30) * -tangent * 0.2f) + (Vector3)arrowOrigin);
                Handles.DrawLine(arrowOrigin, (Quaternion.Euler(0, 0, -30) * -tangent * 0.2f) + (Vector3)arrowOrigin);
                arrowOrigin += tangent * 0.2f;
                Handles.DrawLine(arrowOrigin, (Quaternion.Euler(0, 0, 30) * -tangent * 0.2f) + (Vector3)arrowOrigin);
                Handles.DrawLine(arrowOrigin, (Quaternion.Euler(0, 0, -30) * -tangent * 0.2f) + (Vector3)arrowOrigin);
                arrowOrigin -= tangent * 0.4f;
                Handles.DrawLine(arrowOrigin, (Quaternion.Euler(0, 0, 30) * -tangent * 0.2f) + (Vector3)arrowOrigin);
                Handles.DrawLine(arrowOrigin, (Quaternion.Euler(0, 0, -30) * -tangent * 0.2f) + (Vector3)arrowOrigin);

                arrowOrigin += tangent * 0.2f;
            }
            else
            {
                arrowOrigin += tangent * 0.1f;
                Handles.DrawLine(arrowOrigin, (Quaternion.Euler(0, 0, 30) * tangent * 0.2f) + (Vector3)arrowOrigin);
                Handles.DrawLine(arrowOrigin, (Quaternion.Euler(0, 0, -30) * tangent * 0.2f) + (Vector3)arrowOrigin);
                arrowOrigin += tangent * 0.2f;
                Handles.DrawLine(arrowOrigin, (Quaternion.Euler(0, 0, 30) * tangent * 0.2f) + (Vector3)arrowOrigin);
                Handles.DrawLine(arrowOrigin, (Quaternion.Euler(0, 0, -30) * tangent * 0.2f) + (Vector3)arrowOrigin);
                arrowOrigin -= tangent * 0.3f;
                Handles.DrawLine(arrowOrigin, (Quaternion.Euler(0, 0, 30) * -tangent * 0.2f) + (Vector3)arrowOrigin);
                Handles.DrawLine(arrowOrigin, (Quaternion.Euler(0, 0, -30) * -tangent * 0.2f) + (Vector3)arrowOrigin);
                arrowOrigin -= tangent * 0.2f;
                Handles.DrawLine(arrowOrigin, (Quaternion.Euler(0, 0, 30) * -tangent * 0.2f) + (Vector3)arrowOrigin);
                Handles.DrawLine(arrowOrigin, (Quaternion.Euler(0, 0, -30) * -tangent * 0.2f) + (Vector3)arrowOrigin);

                arrowOrigin += tangent * 0.2f;
            }



            if (!showInSceneDetailed)
            {
                Handles.DrawWireDisc(navPosA.navPoint, Vector3.forward, 0.1f);
                Handles.DrawWireDisc(navPosB.navPoint, Vector3.forward, 0.1f);

                Handles.color = isJumpLinkValid ? Color.green : Color.red;
            }
            else
            {
                Handles.DrawLine(navPosA.navPoint, worldPointA);
                Handles.DrawLine(navPosB.navPoint, worldPointB);
                if (editable)
                {
                    worldPointA = Handles.PositionHandle(worldPointA, Quaternion.identity);
                    worldPointB = Handles.PositionHandle(worldPointB, Quaternion.identity);

                    xSpeedScale = Handles.ScaleSlider(xSpeedScale, arrowOrigin, Vector3.up, Quaternion.identity, HandleUtility.GetHandleSize(arrowOrigin), 0.01f);
                    if (xSpeedScale > 1)
                        xSpeedScale = 1;
                    else if (xSpeedScale < 0)
                        xSpeedScale = 0;
                }

                Vector2 upRight, downRight, upLeft, downLeft;
                float halfWidth = agentWidth / 2;
                upRight = new Vector2(navPosA.navPoint.x - halfWidth, navPosA.navPoint.y + agentHeight);
                upLeft = new Vector2(navPosA.navPoint.x + halfWidth, navPosA.navPoint.y + agentHeight);
                downLeft = new Vector2(navPosA.navPoint.x + halfWidth, navPosA.navPoint.y);
                downRight = new Vector2(navPosA.navPoint.x - halfWidth, navPosA.navPoint.y);
                Handles.DrawLine(upRight, upLeft);
                Handles.DrawLine(upLeft, downLeft);
                Handles.DrawLine(downLeft, downRight);
                Handles.DrawLine(downRight, upRight);

                Vector2 endPointOffset = navPosB.navPoint - navPosA.navPoint;
                Handles.DrawLine(upRight + endPointOffset, upLeft + endPointOffset);
                Handles.DrawLine(upLeft + endPointOffset, downLeft + endPointOffset);
                Handles.DrawLine(downLeft + endPointOffset, downRight + endPointOffset);
                Handles.DrawLine(downRight + endPointOffset, upRight + endPointOffset);

                Handles.DrawWireDisc(navPosA.navPoint, Vector3.forward, 0.05f);
                Handles.DrawWireDisc(navPosB.navPoint, Vector3.forward, 0.05f);

                Handles.DrawWireDisc(downRight, Vector3.forward, 0.1f);
                Handles.DrawWireDisc(downLeft, Vector3.forward, 0.1f);

                Handles.DrawWireDisc(downLeft + endPointOffset, Vector3.forward, 0.1f);
                Handles.DrawWireDisc(downRight + endPointOffset, Vector3.forward, 0.1f);

                Handles.color = isJumpLinkValid ? Color.green : Color.red;

                if (navPosA.navPoint.x > navPosB.navPoint.x)
                {
                    upRight.x -= navPosA.navPoint.x;
                    upLeft.x -= navPosA.navPoint.x;
                    downLeft.x -= navPosA.navPoint.x;
                    downRight.x -= navPosA.navPoint.x;
                    DrawJumpArc(upRight);
                    DrawJumpArc(upLeft);
                    DrawJumpArc(downLeft);
                    DrawJumpArc(downRight);
                }
                else
                {
                    upRight.x -= navPosA.navPoint.x;
                    upLeft.x -= navPosA.navPoint.x;
                    downLeft.x -= navPosA.navPoint.x;
                    downRight.x -= navPosA.navPoint.x;
                    DrawJumpArc(upRight);
                    DrawJumpArc(upLeft);
                    DrawJumpArc(downLeft);
                    DrawJumpArc(downRight);
                }
            }

            Handles.color = Color.white;
        }

        void DrawJumpArc(Vector2 offset)
        {
            Vector2 swapPos;
            Vector2 prevPos = new Vector2(jumpArc.startX, jumpArc.Calc(0)) + offset;
            float absStepWidth = (jumpArc.maxX - jumpArc.minX) / 100;
            float stepWidth = absStepWidth * (jumpArc.endX < jumpArc.startX ? -1f : 1f);

            for (int n = 0; n <= 100; n++)
            {
                swapPos = new Vector2(jumpArc.startX + (n * stepWidth), jumpArc.Calc(n * absStepWidth)) + offset;
                Handles.DrawLine(prevPos, swapPos);
                prevPos = swapPos;
            }
            //Handles.DrawLine(prevPos, new Vector2(link.jumpArc.endX, link.jumpArc.Calc(link.jumpArc.maxX - link.jumpArc.minX)) + origin);
        }

        [Serializable]
        public class JumpLinkSettings
        {
            public bool showDetails = false;
            public bool showInScene = true;
            public bool showInSceneDetailed = true;
        }
    }
}
