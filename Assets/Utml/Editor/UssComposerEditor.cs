using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UssComposer))]
public class UssComposerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var c = ((UssComposer)target);
        c.doc = EditorGUILayout.TextArea(c.doc);

        if (GUILayout.Button("Rebuild"))
        {
            c.Rebuild();
        }
    }
}