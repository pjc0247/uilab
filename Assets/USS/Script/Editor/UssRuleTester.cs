using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UssRuleTester : EditorWindow
{
    private string query;
    private int selections;

    [MenuItem("USS/SelectorTester")]
    public static void ShowRuleTester()
    {
        var window = new UssRuleTester();
        window.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
        query = GUILayout.TextField(query, GUI.skin.FindStyle("ToolbarSeachTextField"));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.LabelField(selections + " object(s) found.");

        if (GUI.changed && string.IsNullOrEmpty(query) == false)
        {
            var conditions = UssParser.ParseConditions(query);
            if (conditions.Length == 0) return;

            Selection.objects = UssStyleModifier.FindObjects(
                UssRoot.FindRootInScene().gameObject, conditions);

            selections = Selection.objects.Length;
        }
    }
}
 