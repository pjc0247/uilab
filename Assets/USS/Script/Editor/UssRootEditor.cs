using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UssRoot))]
public class UssRootEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var t = (UssRoot)target;
        if (GUI.changed && t.ucss != null)
            t.ucssPath = AssetDatabase.GetAssetPath(t.ucss);
    }
}
