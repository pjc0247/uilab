using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UssInspector))]
public class UssInspectorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var t = (UssInspector)target;
        var style = new GUIStyle(GUI.skin.label);
        style.richText = true;
        foreach (var applied in t.applied)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(
                "<b>" + 
                string.Join(" ", applied.conditions.Select(x => x.name).ToArray()) +
                "</b>",
                style);

            var properties = "";
            foreach (var p in applied.properties)
            {
                properties += p.key + ": " + string.Join(" ", p.values.Select(x => x.body).ToArray());
                properties += "\r\n";
            }
            properties = properties.TrimEnd('\r', '\n');

            EditorGUILayout.HelpBox(properties, MessageType.None);
            EditorGUILayout.EndVertical();
        }
    }
}
