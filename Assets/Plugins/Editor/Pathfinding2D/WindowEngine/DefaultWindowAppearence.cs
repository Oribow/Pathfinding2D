using UnityEngine;
using System.Collections;
using UnityEditor;


namespace EditorUI
{
    public class DefaultWindowAppearence
    {

        public static void DrawHeader(string titel)
        {
            Rect headerRect = GUILayoutUtility.GetRect(GUIContent.none, CustomGUI.splitter, GUILayout.Height(EditorStyles.largeLabel.lineHeight));
            Rect backgroundRect = headerRect;
            backgroundRect.yMin = 0;
            EditorGUI.DrawRect(backgroundRect, Color.gray);
            headerRect.yMin = 2;
            headerRect.xMin += 10;
            GUI.Label(headerRect, titel, EditorStyles.largeLabel);
        }
    }
}
