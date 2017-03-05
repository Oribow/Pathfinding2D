using UnityEngine;
using System.Collections;
using System;
using NavData2d.Editor;

namespace Pathfinding2D
{
    [Serializable]
    public class JumpLink : IOffNodeLink
    {
        public float xVel;
        public float jumpForce;
        public float xMin;
        public float xMax;
        public float yMin;
        public float yMax;

        public JumpLink(MetaJumpLink link)
        {
            xVel = link.jumpArc.v ;
            jumpForce = link.jumpArc.j;
            targetPos = link.navPosB;
            startPoint = link.navPosA.navPoint;
            xMin = link.jumpArc.minX;
            xMax = link.jumpArc.maxX;
            yMin = link.jumpArc.minY;
            yMax = link.jumpArc.maxY;
            traversCosts = Mathf.Abs(xMax - xMin);
        }
    }
}
