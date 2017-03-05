using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditorInternal;

namespace NavData2d
{
    [CustomEditor(typeof(NavPositionHolder))]
    public class NavPositionHolder2DInspector : UnityEditor.Editor
    {
        ReorderableList positionList;
        NavigationData2D navData2d;
        SerializedProperty positions;
        int currentSelectedPosition = 0;
        bool shouldConnectEnds;
        bool showPositions;
        Vector2 positionListSrollPos;

        void OnEnable()
        {
            positions = serializedObject.FindProperty("handlePositions");
            if (positionList == null)
                InitPositionReorderableList();
        }

        public override void OnInspectorGUI()
        {
            // Update the serializedProperty
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            //positionListSrollPos = EditorGUILayout.BeginScrollView(positionListSrollPos);
            showPositions = EditorGUILayout.Foldout(showPositions, "Positions");
            if (EditorGUI.EndChangeCheck())
                SceneView.RepaintAll();

            if (showPositions)
            {
                positionList.DoLayoutList();
            }
            //EditorGUILayout.EndScrollView();

            navData2d = (NavigationData2D)EditorGUILayout.ObjectField(navData2d, typeof(NavigationData2D), true);
            GUI.enabled = navData2d != null;
            if (GUILayout.Button("Update Mapping"))
            {
                ((NavPositionHolder)target).MapHandlePositionsToNavData(navData2d);
                serializedObject.ApplyModifiedProperties();
                SceneView.RepaintAll();
            }
            GUI.enabled = true;
            EditorGUILayout.LabelField("in scene drawing options");
            shouldConnectEnds = EditorGUILayout.Toggle("Connect Last & First", shouldConnectEnds);
        }

        public void OnSceneGUI()
        {
            if (!showPositions || positions.arraySize == 0)
                return;

            bool didChangeHappen = false;
            Vector2 oldPos;
            if (shouldConnectEnds)
                oldPos = positions.GetArrayElementAtIndex(positions.arraySize - 1).FindPropertyRelative("handlePosition").vector2Value;
            else
                oldPos = positions.GetArrayElementAtIndex(0).FindPropertyRelative("handlePosition").vector2Value;

            for (int i = 0; i < positions.arraySize; i++)
            {
                var currentPos = positions.GetArrayElementAtIndex(i);
                Vector2 handlePos = currentPos.FindPropertyRelative("handlePosition").vector2Value;
                Handles.Label(handlePos, "Position " + i);
                Handles.DrawLine(handlePos, oldPos);
                Vector2 newPos = Handles.PositionHandle(handlePos, Quaternion.identity);
                if (newPos != handlePos)
                {
                    didChangeHappen = true;
                    currentPos.FindPropertyRelative("handlePosition").vector2Value = newPos;
                    if(navData2d != null)
                    ((NavPositionHolder)target).MapHandlePositionToNavData(i, navData2d);
                }
                oldPos = newPos;

                SerializedProperty navItem = currentPos.FindPropertyRelative("navPosition");
                if (navItem.FindPropertyRelative("navNodeIndex").intValue != -1)
                {
                    Handles.DrawLine(handlePos, navItem.FindPropertyRelative("navPoint").vector2Value);
                }
            }
            if (didChangeHappen)
            {
                positions.serializedObject.ApplyModifiedProperties();
            }
        }

        void InitPositionReorderableList()
        {
            positionList = new ReorderableList(serializedObject, positions, true, true, true, true);
            positionList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty handleItem = positionList.serializedProperty.GetArrayElementAtIndex(index);
                Rect lineRect = rect;
                lineRect.height = EditorGUIUtility.singleLineHeight;

                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(lineRect, handleItem.FindPropertyRelative("handlePosition"), new GUIContent("NavPoint"));
                if (EditorGUI.EndChangeCheck())
                {
                    ((NavPositionHolder)target).MapHandlePositionToNavData(index, navData2d);
                    serializedObject.ApplyModifiedProperties();
                    SceneView.RepaintAll();
                }
                SerializedProperty navItem = handleItem.FindPropertyRelative("navPosition");
                GUI.enabled = false;
                lineRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.Vector2Field(lineRect, "NavPoint", navItem.FindPropertyRelative("navPoint").vector2Value);
                lineRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.IntField(lineRect, "NavNodeIndex", navItem.FindPropertyRelative("navNodeIndex").intValue);
                lineRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.IntField(lineRect, "NavVertIndex", navItem.FindPropertyRelative("navVertIndex").intValue);
                GUI.enabled = true;
            };
            positionList.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "Positions"); };
            positionList.elementHeight = EditorGUIUtility.singleLineHeight * 4 + EditorGUIUtility.standardVerticalSpacing * 4;
            positionList.onAddCallback = (ReorderableList list) =>
            {
                list.serializedProperty.arraySize++;
                var newElement = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1).FindPropertyRelative("navPosition");
                newElement.FindPropertyRelative("navNodeIndex").intValue = -1;
                newElement.FindPropertyRelative("navVertIndex").intValue = -1;
                serializedObject.ApplyModifiedProperties();
            };
            positionList.onRemoveCallback = (ReorderableList list) =>
            {
                positionList.serializedProperty.DeleteArrayElementAtIndex(positionList.index);
                serializedObject.ApplyModifiedProperties();
                Repaint();
            };
        }
    }
}
