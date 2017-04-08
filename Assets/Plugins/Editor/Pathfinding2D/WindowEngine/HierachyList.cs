using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace EditorUI
{
    public class HierarchyList
    {
        const float defaultElementHeight = 10;
        const float defaultElementWidth = 10;
        const float defaultVerticalSpacing = 10;
        const float defaultHorizontalSpacing = 10;

        static void DefaultDrawElement(Rect rect, object data)
        {
            EditorGUI.DrawRect(rect, Color.blue);
            GUILayout.BeginArea(rect);

            if (data == null)
                EditorGUILayout.LabelField("Null");
            else
                EditorGUILayout.LabelField(data.ToString());
            GUILayout.EndArea();
        }

        public delegate void OnDrawElement(Rect rect, object data);


        public Vector2 elementDimension;
        public float verticalSpacing;
        public float horizontalSpacing;
        public OnDrawElement elementDrawCallback;

        IHierarchyElement rootElement;
        float elementGroupVerticalSpacing = 10;

        float totalHeight = 0;
        int hierachyDepth;
        bool needsRecalculation;

        public HierarchyList(IHierarchyElement rootElement)
        {
            this.rootElement = rootElement;
            elementDimension = new Vector2(defaultElementWidth, defaultElementHeight);
            verticalSpacing = defaultVerticalSpacing;
            horizontalSpacing = defaultVerticalSpacing;
            needsRecalculation = true;
        }

        public void DoLayout()
        {
            if (needsRecalculation && rootElement != null)
                CalculateElementPositions();

            if (elementDrawCallback == null)
                elementDrawCallback = DefaultDrawElement;

            GUILayoutUtility.GetRect(hierachyDepth * (horizontalSpacing + elementDimension.x), totalHeight + verticalSpacing);

            DrawElement(rootElement, new Vector2(5, totalHeight / 2));
        }

        void DrawElement(IHierarchyElement parent, Vector2 position)
        {
            elementDrawCallback.Invoke(new Rect(position.x, position.y, elementDimension.x, elementDimension.y), parent.Data);
            EditorGUI.DrawRect(new Rect(position.x - horizontalSpacing / 2, position.y + elementDimension.y / 2, horizontalSpacing / 2, 1f), Color.black);
            if (parent.ChildrenCount > 0)
            {
                EditorGUI.DrawRect(new Rect(position.x + elementDimension.x, position.y + elementDimension.y / 2, horizontalSpacing / 2, 1f), Color.black);
                if (parent.ChildrenCount > 1)
                {
                    EditorGUI.DrawRect(new Rect(position.x + elementDimension.x + horizontalSpacing / 2,
                        position.y + parent.GetChildrenAt(0).HeightOffsetRelativeToParent + elementDimension.y / 2,
                        1,
                        Mathf.Abs(parent.GetChildrenAt(0).HeightOffsetRelativeToParent) + parent.GetChildrenAt(parent.ChildrenCount -1).HeightOffsetRelativeToParent), Color.black);
                }
            }
            foreach (var child in parent.Children)
            {
                DrawElement(child, new Vector2(position.x + horizontalSpacing + elementDimension.x,
                    position.y + child.HeightOffsetRelativeToParent));
            }
        }

        void CalculateElementPositions()
        {
            hierachyDepth = 0;
            totalHeight = GetChildrenHeight(rootElement, 0);
            needsRecalculation = false;
        }

        float GetChildrenHeight(IHierarchyElement parent, int hierachyDepth)
        {
            hierachyDepth++;
            if (hierachyDepth > this.hierachyDepth)
                this.hierachyDepth = hierachyDepth;

            if (parent.ChildrenCount == 0)
            {
                parent.HeightOffsetRelativeToParent = 0;
                return elementDimension.y;
            }

            float[] heights = new float[parent.ChildrenCount];
            float totalHeight = 0;
            for (int iChild = 0; iChild < parent.ChildrenCount; iChild++)
            {
                heights[iChild] += GetChildrenHeight(parent.GetChildrenAt(iChild), hierachyDepth);
                totalHeight += heights[iChild] + verticalSpacing;
            }
            totalHeight -= verticalSpacing;
            float middle = totalHeight / 2;
            float currentOffset = -middle;
            for (int iChild = 0; iChild < parent.ChildrenCount; iChild++)
            {
                parent.GetChildrenAt(iChild).HeightOffsetRelativeToParent = currentOffset + heights[iChild] / 2;
                currentOffset += heights[iChild] + verticalSpacing;
            }
            return totalHeight;
        }
    }
}
