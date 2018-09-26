using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class UtmlElementName : Attribute
{
    public string name;

    public UtmlElementName(string name)
    {
        this.name = name;
    }
}

public class UtmlElement : MonoBehaviour
{
    public Rect padding;

    public RectTransform rt
    {
        get { return GetComponent<RectTransform>(); }
    }

    public virtual void Construct(UtmlConstructionData cd)
    {
    }
    public virtual void PostConstruct(UtmlConstructionData cd)
    {
    }

#if UNITY_EDITOR
    private void DrawRect(Rect rect, Vector3 position, Color color)
    {
        Vector3[] points = new Vector3[] {
              new Vector3(rect.min.x + position.x,
                rect.min.y + position.y),
              new Vector3(rect.min.x + position.x,
                rect.min.y + position.y + rect.height),
              new Vector3(rect.min.x + position.x,
                rect.min.y + position.y + rect.height),
              new Vector3(rect.max.x + position.x,
                rect.max.y + position.y),
              new Vector3(rect.max.x + position.x,
                rect.max.y + position.y),
              new Vector3(rect.max.x + position.x,
                rect.max.y + position.y - rect.height),
              new Vector3(rect.max.x + position.x,
                rect.max.y + position.y - rect.height),
              new Vector3(rect.min.x + position.x,
                rect.min.y + position.y),
            };
        Handles.color = color;
        Handles.DrawAAPolyLine(2, points);
    }
    private static void drawString(string text, Vector3 worldPos, Color? colour = null)
    {
        UnityEditor.Handles.BeginGUI();

        var restoreColor = GUI.color;

        if (colour.HasValue) GUI.color = colour.Value;
        var view = UnityEditor.SceneView.currentDrawingSceneView;
        Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);

        if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0)
        {
            GUI.color = restoreColor;
            UnityEditor.Handles.EndGUI();
            return;
        }

        var style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = 35;
        Vector2 size = style.CalcSize(new GUIContent(text));
        GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y - (size.y) + view.position.height, size.x, size.y), text, style);
        GUI.color = restoreColor;
        UnityEditor.Handles.EndGUI();
    }
    private static void drawString(string text, Vector2 position, Color? colour = null)
    {
        UnityEditor.Handles.BeginGUI();

        var restoreColor = GUI.color;

        if (colour.HasValue) GUI.color = colour.Value;
        var view = UnityEditor.SceneView.currentDrawingSceneView;

        var style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = 35;
        Vector2 size = style.CalcSize(new GUIContent(text));
        GUI.Label(new Rect(position.x, position.y, size.x, size.y), text, style);
        GUI.color = restoreColor;
        UnityEditor.Handles.EndGUI();
    }
    void OnDrawGizmosSelected()
    {
        DrawRect(rt.rect, rt.position, Color.red);

        if (Selection.objects[0] != gameObject)
            return;

        var paddingRect = new Rect(rt.rect);
        paddingRect.x -= padding.x;
        paddingRect.width += padding.width + padding.x;
        paddingRect.y -= padding.height - padding.y;
        paddingRect.height += padding.height;
        DrawRect(paddingRect, rt.position, Color.yellow);

        drawString(padding.x.ToString(),
            transform.position +
            new Vector3(paddingRect.min.x - 10, rt.rect.y + rt.rect.height/2, rt.position.z),
            Color.yellow);
        drawString(padding.width.ToString(),
            transform.position +
            new Vector3(paddingRect.max.x + 10, rt.rect.y + rt.rect.height / 2, rt.position.z),
            Color.yellow);

        drawString(padding.height.ToString(),
            transform.position +
            new Vector3(rt.rect.x + rt.rect.width / 2, paddingRect.min.y - 10, rt.position.z),
            Color.yellow);
        drawString(padding.y.ToString(),
            transform.position +
            new Vector3(rt.rect.x + rt.rect.width / 2, paddingRect.max.y + 10, rt.position.z),
            Color.yellow);
    }
#endif
}
