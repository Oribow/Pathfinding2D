using UnityEngine;
using System.Collections;
using UnityEditor;

public class ReadOnlyAttribute : PropertyAttribute
{

}

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property,
                                            GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position,
                               SerializedProperty property,
                               GUIContent label)
    {
        bool oldValue = GUI.enabled;
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = oldValue;
    }
}

public class ForceWritableAttribute : PropertyAttribute
{

}

[CustomPropertyDrawer(typeof(ForceWritableAttribute))]
public class ForceWritableDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property,
                                            GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position,
                               SerializedProperty property,
                               GUIContent label)
    {
        bool oldValue = GUI.enabled;
        GUI.enabled = true;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = oldValue;
    }
}