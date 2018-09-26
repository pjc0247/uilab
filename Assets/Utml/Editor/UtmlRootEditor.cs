using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UtmlRoot))]
public class UtmlRootEditor : Editor
{ 
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var t = (UtmlRoot)target;
        if (GUI.changed && t.utml != null)
            t.utmlPath = AssetDatabase.GetAssetPath(t.utml);
    }
} 