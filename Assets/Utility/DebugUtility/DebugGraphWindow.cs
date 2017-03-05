using UnityEngine;
using UnityEditor;
using System.Collections;

public class DebugGraphWindow : EditorWindow {

    [MenuItem("Window/DebugGraphViewer")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        DebugGraphWindow window = (DebugGraphWindow)EditorWindow.GetWindow(typeof(DebugGraphWindow));
        window.Show();
    }

    DebugGraph debugGraph;
    GraphDataDrawer graphDrawer;
    Vector2 scrollPos;

    void OnEnable()
    {
        debugGraph = DebugGraph.Instance;
        graphDrawer = new GraphDataDrawer();
    }

    void OnDestroy()
    {
        debugGraph.ClearStartedGraphs();
    }

    void OnGUI()
    {
        if (debugGraph.IsEmpty)
        {
            GUILayout.Label("Nothing to show.");
            return;
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
        GUIStyle labelStyle = new GUIStyle(EditorStyles.label);

        foreach (var keyPair in debugGraph)
        {
            if (keyPair.isHidden)
            {
                EditorGUILayout.BeginHorizontal();
               
                EditorGUILayout.LabelField(keyPair.name);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("<o>", GUILayout.ExpandWidth(false)))
                {
                    keyPair.isHidden = false;
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                Rect pos = EditorGUILayout.GetControlRect(false, 80, GUILayout.Width(position.width - 10));
                pos.xMin += 5;
                pos.xMax -= 10;
                graphDrawer.DrawGraph(pos, keyPair.graph);

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(keyPair.name);
                EditorGUIUtility.labelWidth = 15;
                for (int iChannel = 0; iChannel < keyPair.graph.channelNames.Length; iChannel++)
                {
                    float labelWidth = GUI.skin.label.CalcSize(new GUIContent(keyPair.graph.channelNames[iChannel] + "0.000")).x;
                    EditorGUILayout.LabelField(keyPair.graph.channelNames[iChannel] + ":", keyPair.graph.channels[iChannel]._data[0].ToString("0.000"), GUILayout.Width(labelWidth));
                    pos = GUILayoutUtility.GetLastRect();
                    labelWidth = pos.width;
                    pos.width = pos.height;

                    EditorGUI.DrawRect(pos, keyPair.graph.channels[iChannel]._color);
                    pos.width = labelWidth;
                    labelStyle.normal.textColor = new Color(1 - keyPair.graph.channels[iChannel]._color.r, 1 - keyPair.graph.channels[iChannel]._color.g, 1 - keyPair.graph.channels[iChannel]._color.b);
                    EditorGUI.LabelField(pos, keyPair.graph.channelNames[iChannel] + ": ", labelStyle);
                }
                EditorGUIUtility.labelWidth = 0;
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(keyPair.isPaused ? "|>" : "||", GUILayout.ExpandWidth(false)))
                {
                    keyPair.isPaused = !keyPair.isPaused;
                }
                if (GUILayout.Button("<->", GUILayout.ExpandWidth(false)))
                {
                    keyPair.isHidden = true;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.EndScrollView();
            Repaint(); 
    }
}
