using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NavSurface2d))]
public class NavSurface2dInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Bake"))
        {
            var navSurface = (NavSurface2d)target;
            //navSurface.Bake();
        }
    }
}
