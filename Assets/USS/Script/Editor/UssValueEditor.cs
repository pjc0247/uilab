using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UssValueEditor : EditorWindow
{
    private int tab;

    private Texture2D noResultTex;

    private GUIStyle valueNameStyle;

    private string query;
    private int queryResults = 0;
    private Vector2 colorScroll;

    [MenuItem("USS/Instant Editor")]
    public static void ShowValueEditor()
    {
        var window = new UssValueEditor();
        window.titleContent = new GUIContent(
            "InstantEditor",
            AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/USS/Resources/penguin.png"));
        window.Show();
    }

    void OnEnable()
    {
        noResultTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/USS/Resources/noresult.png");
             
        valueNameStyle = EditorStyles.boldLabel;
    }
    public void OnGUI()
    {
        UssAutoRefresh.EnsureLastUcssLoaded();

        tab = GUILayout.Toolbar(tab, new string[] { "Color", "Number", "String" });
        EditorGUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
        query = GUILayout.TextField(query, GUI.skin.FindStyle("ToolbarSeachTextField"));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        queryResults = 0;
        if (tab == 0)
            DrawColors();
        else if (tab == 1)
            DrawNumbers();
        else if (tab == 2)
            DrawStrings();

        if (queryResults == 0)
        {
            GUILayout.Box(noResultTex, GUILayout.Width(100), GUILayout.Height(100));
            EditorGUILayout.LabelField("No results");
        }

        if (GUI.changed)
        {
            var root = UssRoot.FindRootInScene();
            UssStyleModifier.Apply(root.gameObject);
            EditorUtility.SetDirty(root.gameObject);
        }
    }

    private void DrawColors()
    {
        colorScroll = EditorGUILayout.BeginScrollView(colorScroll);
        foreach (var v in UssValues.values.Where(x => x.Value is UssColorValue))
        {
            if (string.IsNullOrEmpty(query) == false &&
                v.Key.Contains(query) == false)
                continue;

            var colorValue = (UssColorValue)v.Value;
            var str = ColorUtility.ToHtmlStringRGBA(colorValue.value);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(v.Key, valueNameStyle);
            colorValue.value = EditorGUILayout.ColorField(colorValue.value);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("References"))
            {
                Selection.objects = UssStyleModifier.GetReferences(GameObject.Find("Canvas"), v.Key);
            }
            EditorGUILayout.SelectableLabel("#" + str);
            EditorGUILayout.EndHorizontal();

            queryResults++;
        }
        EditorGUILayout.EndScrollView();
    }
    private void DrawNumbers()
    {
        colorScroll = EditorGUILayout.BeginScrollView(colorScroll);
        foreach (var v in UssValues.values.Where(x => (x.Value is UssFloatValue || x.Value is UssIntValue)))
        {
            if (string.IsNullOrEmpty(query) == false &&
                v.Key.Contains(query) == false)
                continue;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(v.Key, valueNameStyle);
            if (v.Value is UssFloatValue)
            {
                var floatValue = (UssFloatValue)v.Value;
                floatValue.value = EditorGUILayout.FloatField(floatValue.value);
            }
            else
            {
                var intValue = (UssIntValue)v.Value;
                intValue.value = EditorGUILayout.IntField(intValue.value);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("References"))
                Selection.objects = UssStyleModifier.GetReferences(GameObject.Find("Canvas"), v.Key);
            EditorGUILayout.EndHorizontal();

            queryResults++;
        }
        EditorGUILayout.EndScrollView();
    }
    private void DrawStrings()
    {
        colorScroll = EditorGUILayout.BeginScrollView(colorScroll);
        foreach (var v in UssValues.values.Where(x => (x.Value is UssStringValue)))
        {
            if (string.IsNullOrEmpty(query) == false &&
                v.Key.Contains(query) == false)
                continue;

            queryResults++;
        }
        EditorGUILayout.EndScrollView();
    }
}
