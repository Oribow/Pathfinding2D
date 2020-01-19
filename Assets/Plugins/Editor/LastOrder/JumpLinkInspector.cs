using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OffNavLineLink))]
public class JumpLinkInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Update"))
        {
            var navSurface = (OffNavLineLink)target;
            navSurface.UpdateLink();
        }
    }
}
