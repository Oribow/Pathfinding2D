using UnityEditorInternal;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace NavGraph.Build
{
    class BuildProcessSaveSelectionWindow : EditorWindow
    {
        public delegate void OnSelectionChanged(BuildProcessSave item);
        public OnSelectionChanged selectionChangedHandler;

        ReorderableList saveFileList;
        List<BuildProcessSave> saveFiles;
        int selection = -1;
        Vector2 globalScroll;

        void OnEnable()
        {
            saveFiles = new List<BuildProcessSave>(GameObject.FindObjectsOfType<BuildProcessSave>());
            saveFiles.Sort((BuildProcessSave a, BuildProcessSave b) => {
                return string.Compare(a.name, b.name);
            });
            InitList();
            titleContent = new GUIContent("BuildSaveSelector");
        }

        void OnGUI()
        {
            saveFileList.index = 0;
            globalScroll = EditorGUILayout.BeginScrollView(globalScroll);
            saveFileList.DoLayoutList();
            EditorGUILayout.EndScrollView();
        }

        void InitList()
        {
            saveFileList = new ReorderableList(saveFiles, typeof(BuildProcessSave), false, true, true, true);
            saveFileList.drawElementBackgroundCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                if (index == selection && selection >= 0)
                    EditorGUI.DrawRect(rect, Color.blue);
            };
            saveFileList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = (BuildProcessSave)saveFileList.list[index];
                if (element == null) //Seems like the user somehow deleted a related GameObject.
                {
                    saveFiles.Clear();
                    saveFiles.AddRange(GameObject.FindObjectsOfType<BuildProcessSave>());
                    Repaint();
                    return;
                }
                rect.y += 5;
                rect.height -= 10;

                rect.width -= 130;
                element.name = EditorGUI.TextField(rect, element.name);

                rect.x += rect.width + 10;
                rect.width = 60;
                if (GUI.Button(rect, "Select", EditorStyles.miniButtonMid))
                {
                    selection = index;
                    if (selectionChangedHandler != null)
                        selectionChangedHandler.Invoke(element);
                }
                rect.x += rect.width;
                rect.width = 60;
                if (GUI.Button(rect, "Duplicate", EditorStyles.miniButtonRight))
                {
                    saveFiles.Add(GameObject.Instantiate(element.gameObject).GetComponent<BuildProcessSave>());
                    saveFiles[saveFiles.Count - 1].name = GetDefaultName(saveFiles[saveFiles.Count - 1].name);
                }
            };
            saveFileList.onAddCallback = (ReorderableList list) =>
            {
                saveFileList.index = 0;
                list.list.Add(BuildProcessSave.CreateNewInstance(GetDefaultName("BuildSave")));
            };
            saveFileList.elementHeight = EditorGUIUtility.singleLineHeight + 10;
            saveFileList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Save files");
            };
            saveFileList.onRemoveCallback = (ReorderableList list) =>
            {
                if (selection == -1)
                    return;
                var element = (BuildProcessSave)saveFileList.list[selection];
                if (EditorUtility.DisplayDialog("Delete save file", "Do you really want to delete " + element.name + "?", "Delete"))
                {
                    GameObject.DestroyImmediate(element.gameObject);
                    saveFileList.list.RemoveAt(selection);
                    selection--;
                }
            };
        }

        string GetDefaultName(string rootName)
        {
            string name = rootName;
            int suffix = 0;
            do
            {
                foreach (var file in saveFiles)
                {
                    if (file.name.Equals(name))
                    {
                        goto Next;
                    }
                }
                break;

                Next:
                suffix++;
                name = rootName + suffix;
                continue;
            } while (true);
            return name;
        }
    }
}
