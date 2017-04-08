using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EditorUI
{
    public class WindowOverlappingBox
    {
        public enum Placement { UpperLeftCorner, LowerLeftCorner, UpperRightCorner, LowerRightCorner }
        public delegate void OnDrawContent(Rect rect);

        public Action DrawLayoutContentCallback;
        public OnDrawContent DrawContentCallback;

        public Placement windowPlacement;
        public Color backgroundColor;
        public Color borderColor;
        public float borderSize;
        public bool useAbsoluteDimensions;
        public Vector2 boxDimensions;

        public WindowOverlappingBox(Placement placement, bool useAbsoluteDimensions, Vector2 boxDimensions, float borderSize)
        {
            this.windowPlacement = placement;
            this.boxDimensions = boxDimensions;
            this.borderSize = borderSize;
            this.borderColor = Color.black;
            this.backgroundColor = Color.white;
            this.useAbsoluteDimensions = useAbsoluteDimensions;
        }

        public void DoLayout(Rect windowRect)
        {
            Rect boxRect = CalcBoxRect(windowRect);
            EditorGUI.DrawRect(boxRect, borderColor);

            Rect contentRect = boxRect;
            contentRect.xMin += borderSize;
            contentRect.yMin += borderSize;
            contentRect.xMax -= borderSize;
            contentRect.yMax -= borderSize;
            EditorGUI.DrawRect(contentRect, backgroundColor);

            if (DrawLayoutContentCallback != null)
            {
                GUILayout.BeginArea(contentRect);
                DrawLayoutContentCallback();
                GUILayout.EndArea();
            }
            if (DrawContentCallback != null)
            {
                DrawContentCallback(contentRect);
            }
        }

        Rect CalcBoxRect(Rect windowRect)
        {
            Rect boxRect = new Rect();
            Vector2 boxDimensions;
            if (!useAbsoluteDimensions)
            {
                boxDimensions = new Vector2(this.boxDimensions.x * windowRect.width, this.boxDimensions.y * windowRect.height);
            }
            else
            {
                boxDimensions = this.boxDimensions;
            }
            switch (windowPlacement)
            {
                case Placement.LowerLeftCorner:
                    boxRect = new Rect(0, windowRect.height - boxDimensions.y, boxDimensions.x, boxDimensions.y);
                    break;
                case Placement.UpperLeftCorner:
                    boxRect = new Rect(0, 0, boxDimensions.x, boxDimensions.y);
                    break;
                case Placement.UpperRightCorner:
                    boxRect = new Rect(windowRect.width - boxDimensions.x, 0, boxDimensions.x, boxDimensions.y);
                    break;
                case Placement.LowerRightCorner:
                    boxRect = new Rect(windowRect.width - boxDimensions.x, windowRect.height - boxDimensions.y, boxDimensions.x, boxDimensions.y);
                    break;
            }
            return boxRect;
        }
    }
}
