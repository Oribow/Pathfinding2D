using UnityEngine;
using System.Collections.Generic;

namespace NavGraph
{
    public class NavPositionHolder : MonoBehaviour
    {
        [SerializeField]
        public NavPositionHandle[] handlePositions = new NavPositionHandle[0];

        public void MapHandlePositionsToNavData(NavigationData2D navData2d)
        {
            for (int iHandlePos = 0; iHandlePos < handlePositions.Length; iHandlePos++)
            {
                var hPos = handlePositions[iHandlePos];
                navData2d.SamplePoint(hPos.handlePosition, out hPos.navPosition);
            }
        }

        public void MapHandlePositionToNavData(int index, NavigationData2D navData2d)
        {
            var hPos = handlePositions[index];
            navData2d.SamplePoint(hPos.handlePosition, out hPos.navPosition);
        }

        [System.Serializable]
        public class NavPositionHandle
        {
            public Vector2 handlePosition;
            public NavPosition navPosition;

            public NavPositionHandle(Vector2 handlePosition, NavPosition navPosition)
            {
                this.handlePosition = handlePosition;
                this.navPosition = navPosition;
            }
        }
    }

    [System.Serializable]
    public class NavPosition
    {
        public Vector2 navPoint;
        public int navNodeIndex;
        public int navVertIndex;

        public NavPosition(Vector2 navPoint, int navNodeIndex, int navVertIndex)
        {
            this.navPoint = navPoint;
            this.navNodeIndex = navNodeIndex;
            this.navVertIndex = navVertIndex;
        }

        public NavPosition()
        {
            this.navPoint = Vector2.zero;
            this.navNodeIndex = -1;
            this.navVertIndex = -1;
        }
    }
}
