using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HtmlAgilityPack;

[DisallowMultipleComponent]
public class UtmlRoot : MonoBehaviour
{
    //[HideInInspector]
    public string utmlPath;
    public Object utml;

    private string inlineStyle;

    void Start ()
    {
        
	}

    public void Rebuild(string doc)
    {
        for (int i = 0; i < transform.childCount; i++)
            DestroyImmediate(transform.GetChild(0).gameObject);

        var html = new HtmlDocument();
        html.LoadHtml(doc);

        inlineStyle = "";
        foreach (var child in html.DocumentNode.ChildNodes)
            ComposeNode(child, transform);

        if (string.IsNullOrEmpty(inlineStyle) == false)
            UssStyleModifier.ApplyUss(inlineStyle);
        UssStyleModifier.LoadUss(UssRoot.FindRootInScene().ucssPath);
    }

    private void ComposeNode(HtmlNode node, Transform parent)
    {
        if (node.NodeType == HtmlNodeType.Text)
            return;

        Transform tf = parent;

        if (node.Name == "style")
            inlineStyle += node.InnerText;
        else
        {
            tf = UssFactory.CreateObject(node, parent).transform;
            foreach (var child in node.ChildNodes)
                ComposeNode(child, tf);
        }
    }
}
