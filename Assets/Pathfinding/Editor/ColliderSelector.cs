using UnityEngine;
using System.Collections;
using System;
using UnityEditor;
using UnityEditorInternal;
using NavMesh2D.Core;

namespace NavData2d.Editor
{
    public class ColliderSelector : ITab
    {
        enum DebugOptions { Raw, RawAndFilled, Non };

        public bool IsEnabled
        {
            get
            {
                return navBuilder.GlobalBuildContainer != null;
            }
        }

        public string TabHeader
        {
            get
            {
                return "ColSelector";
            }
        }

        ColliderSet colliderSet { get { return navBuilder.GlobalBuildContainer.colliderSet; } set { navBuilder.GlobalBuildContainer.colliderSet = value; } }

        ReorderableList colliderListContainer;

        [SerializeField]
        Vector2 colliderListScrollPos;
        [SerializeField]
        LayerMask colliderLayerMask;
        [SerializeField]
        DebugOptions debugOption;

        INavDataBuilder navBuilder;

        public ColliderSelector(INavDataBuilder navBuilder)
        {
            this.navBuilder = navBuilder;
        }

        public void OnSelected()
        {
            if (colliderSet == null)
                colliderSet = new ColliderSet();
            else
                colliderSet.TriggerGeometryVertsUpdate();
            if (colliderListContainer == null)
                InitColliderListUI();
            
        }

        public void OnUnselected()
        {
            
        }

        public void OnGUI()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add static"))
            {
                colliderSet.AddAllStaticCollider();
            }
            if (GUILayout.Button("Add Selection"))
            {
                colliderSet.AddSelectedCollider();
            }
            if (GUILayout.Button("Remove All"))
            {
                colliderSet.colliderList.Clear();
                colliderSet.TriggerGeometryVertsUpdate();
            }
            GUILayout.EndHorizontal();

            colliderLayerMask = CustomEditorFields.LayerMaskField("Layer Mask", colliderLayerMask);
            if (GUILayout.Button("Add Layer"))
            {
                colliderSet.AddColliderOnLayer(colliderLayerMask);
            }
            colliderSet.CircleColliderVertCount = EditorGUILayout.IntField("CircleColliderVerts", colliderSet.CircleColliderVertCount);

            colliderListScrollPos = EditorGUILayout.BeginScrollView(colliderListScrollPos);
            colliderListContainer.DoLayoutList();
            EditorGUILayout.EndScrollView();

            EditorGUI.BeginChangeCheck();
            debugOption = (DebugOptions)EditorGUILayout.EnumPopup("Debug Type", debugOption);
            if (EditorGUI.EndChangeCheck() && colliderListContainer.index == -1)
                SceneView.RepaintAll();
        }

        public void OnSceneGUI(SceneView sceneView)
        {
            if (colliderSet == null || colliderSet.colliderVerts == null)
            {
                return;
            }
            if (colliderListContainer.index != -1)
            {
                Handles.color = Color.blue;
                Vector2d[] vertSet = colliderSet.colliderVerts[colliderListContainer.index];
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
                foreach (Vector2d[] vertSet in colliderSet.colliderVerts)
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
                foreach (Vector2d[] vertSet in colliderSet.colliderVerts)
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
                    colliderSet.colliderList.RemoveAt(index);
                    colliderSet.TryUpdateGeometryVerts();
                    navBuilder.TriggerRepaint();
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
