using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;

namespace NavGraph.Build
{
    [System.Serializable]
    public class ColliderSelector : BuildStepWindow
    {
        enum DebugOptions { Raw, RawAndFilled, Non };

        ColliderSet colliderSet { get { return BuildSave.ColliderSet; } }

        ReorderableList colliderListContainer;

        [SerializeField]
        Vector2 colliderListScrollPos;
        [SerializeField]
        LayerMask colliderLayerMask;
        [SerializeField]
        DebugOptions debugOption;

        protected override void InitThisWindow()
        {
            colliderSet.TriggerGeometryVertsUpdate();
            if (colliderListContainer == null)
                InitColliderListUI();

        }

        void OnEnable()
        {
            this.titleContent = new GUIContent("ColliderSelector");
        }

        protected override void DrawCustomGUI()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add static"))
            {
                colliderSet.AddAllStaticCollider();
                UpdateData();
            }
            if (GUILayout.Button("Add Selection"))
            {
                colliderSet.AddSelectedCollider();
                UpdateData();
            }
            if (GUILayout.Button("Remove All"))
            {
                colliderSet.RemoveAll();
                UpdateData();
            }
            GUILayout.EndHorizontal();

            colliderLayerMask = CustomEditorFields.LayerMaskField("Layer Mask", colliderLayerMask);
            if (GUILayout.Button("Add Layer"))
            {
                colliderSet.AddColliderOnLayer(colliderLayerMask);
                UpdateData();
            }
            EditorGUI.BeginChangeCheck();
            colliderSet.CircleColliderVertCount = EditorGUILayout.IntField("CircleColliderVerts", colliderSet.CircleColliderVertCount);
            colliderSet.DecimalPlacesOfCoords = EditorGUILayout.IntSlider("DecimalPlacesOfCoords", colliderSet.DecimalPlacesOfCoords, 0, 15);
            if (EditorGUI.EndChangeCheck())
                UpdateData();

            colliderListScrollPos = EditorGUILayout.BeginScrollView(colliderListScrollPos);
            colliderListContainer.DoLayoutList();
            EditorGUILayout.EndScrollView();

            EditorGUI.BeginChangeCheck();
            debugOption = (DebugOptions)EditorGUILayout.EnumPopup("Debug Type", debugOption);
            if (EditorGUI.EndChangeCheck() && colliderListContainer.index == -1)
                SceneView.RepaintAll();
        }

        void UpdateData()
        {
            BuildWin.MarkFollowingStepsOutdated(0);
            BuildWin.UpdateBuildStepInformation();
            SceneView.RepaintAll();
            EditorUtility.SetDirty(colliderSet);
        }

        protected override void DrawCustomSceneGUI(SceneView sceneView)
        {
            if (colliderSet == null || colliderSet.colliderVerts == null)
            {
                return;
            }
            if (colliderListContainer.index != -1)
            {
                Handles.color = Color.blue;
                Vector2[] vertSet = colliderSet.colliderVerts[colliderListContainer.index];
                Vector3[] dummyArray;
                dummyArray = new Vector3[vertSet.Length];
                for (int iVert = 0; iVert < vertSet.Length; iVert++)
                    dummyArray[iVert] = (Vector2)vertSet[iVert];
                Handles.DrawAAPolyLine(5f, dummyArray);
                Handles.DrawAAPolyLine(5f, dummyArray[0], dummyArray[dummyArray.Length - 1]);
            }
            else if (debugOption == DebugOptions.Raw)
            {
                Handles.color = Color.blue;
                Vector3[] dummyArray;
                foreach (Vector2[] vertSet in colliderSet.colliderVerts)
                {
                    dummyArray = new Vector3[vertSet.Length];
                    for (int iVert = 0; iVert < vertSet.Length; iVert++)
                        dummyArray[iVert] = (Vector2)vertSet[iVert];
                    Handles.DrawAAPolyLine(5f, dummyArray);
                    Handles.DrawAAPolyLine(5f, dummyArray[0], dummyArray[dummyArray.Length - 1]);
                }
            }
            else if (debugOption == DebugOptions.RawAndFilled)
            {
                Handles.color = Color.blue;
                Vector3[] dummyArray;
                foreach (Vector2[] vertSet in colliderSet.colliderVerts)
                {
                    dummyArray = new Vector3[vertSet.Length + 1];
                    for (int iVert = 0; iVert < vertSet.Length; iVert++)
                        dummyArray[iVert] = (Vector2)vertSet[iVert];
                    dummyArray[dummyArray.Length - 1] = dummyArray[0];
                    Handles.DrawAAConvexPolygon(dummyArray);
                }
            }
        }

        void InitColliderListUI()
        {
            colliderListContainer = new ReorderableList(colliderSet.colliderList, typeof(Collider2D), false, true, true, true);
            colliderListContainer.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                if (index >= colliderListContainer.list.Count)
                    return;

                rect.y += EditorGUIUtility.singleLineHeight / 2;
                rect.height -= EditorGUIUtility.singleLineHeight;
                rect.width -= 25;

                var element = (Collider2D)colliderListContainer.list[index];

                EditorGUI.BeginChangeCheck();
                element = (Collider2D)EditorGUI.ObjectField(rect, element, typeof(Collider2D), true);
                colliderListContainer.list[index] = element;

                rect.x += rect.width + 5;
                rect.width = 20;
                if (GUI.Button(rect, "X"))
                {
                    colliderSet.RemoveAt(index);
                    Repaint();
                }
                if (EditorGUI.EndChangeCheck())
                {
                    colliderSet.RemoveDuplicates();
                }
            };
            colliderListContainer.onAddCallback = (ReorderableList list) =>
            {
                list.list.Add(null);
            };
            colliderListContainer.elementHeight = EditorGUIUtility.singleLineHeight * 2;
            colliderListContainer.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Collider  (" + colliderSet.colliderList.Count + ")");
            };
            colliderListContainer.onSelectCallback = (ReorderableList list) =>
            {
                SceneView.RepaintAll();
            };
        }
    }
}
