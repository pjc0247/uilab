using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UssInstantEval : EditorWindow
{
    private string script;
    private string prevSelectionText;

    [MenuItem("USS/InstantEval")]
    public static void ShowInstantEval()
    {
        var window = new UssInstantEval();
        window.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("INSTANT EVAL");
        if (GUILayout.Button("run!"))
        {
            /*
            var result = UssParser.Parse(script);
            UssStyleModifier.Apply(
                UssRoot.FindRootInScene().gameObject,
                result.styles);
            */
            UssStyleModifier.LoadUss(script);
        }
        EditorGUILayout.EndHorizontal();

        script = GUI.TextArea(new Rect(0, 30, position.width, 300), script);

        var editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
        if (string.IsNullOrEmpty(editor.SelectedText) == false)
        {
            if (prevSelectionText == editor.SelectedText) return;

            var result = UssParser.ParseConditions(editor.SelectedText);
            Selection.objects = UssStyleModifier.FindObjects(
                UssRoot.FindRootInScene().gameObject,
                result);

            prevSelectionText = editor.SelectedText;
        }
    }
}
