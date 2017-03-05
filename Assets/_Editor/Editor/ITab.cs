using UnityEngine;
using System.Collections;
using UnityEditor;

public interface ITab {

	string TabHeader { get; }
    bool IsEnabled { get; }

    void OnSelected();
    void OnUnselected();
    void OnGUI();
    void OnSceneGUI(SceneView sceneView);
}
