using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(JumpLink))]
public class JumpLinkInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Update"))
        {
            var navSurface = (JumpLink)target;
            navSurface.UpdateLink();
        }
    }
}
