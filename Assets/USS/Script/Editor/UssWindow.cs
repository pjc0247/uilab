using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UssWindow : EditorWindow
{
    private Texture2D cssTex;

    [MenuItem("USS/Show editor")]
    public static void ShowEditor()
    {
        var window = new UssWindow();
        window.Show();
    }

    void OnEnable()
    {
        cssTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/USS/Resources/css.png");
    }

    void OnGUI()
    {
        EditorGUI.DrawPreviewTexture(new Rect(position.width / 2 - 50, 20, 100, 100), cssTex);
        GUILayout.Space(140);
    }
}
