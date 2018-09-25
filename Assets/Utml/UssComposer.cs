using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HtmlAgilityPack;

public class UssComposer : MonoBehaviour
{
    public string doc = "";

	void Start ()
    {
        
	}

    public void Rebuild()
    {
        for (int i = 0; i < transform.childCount; i++)
            DestroyImmediate(transform.GetChild(0).gameObject);

        var html = new HtmlDocument();
        html.LoadHtml(doc);

        foreach (var child in html.DocumentNode.ChildNodes)
            ComposeNode(child, transform);
    }

    private void ComposeNode(HtmlNode node, Transform parent)
    {
        if (node.NodeType == HtmlNodeType.Text)
            return;

        Transform tf = parent;

        tf = UssFactory.CreateObject(node, parent).transform;

        foreach (var child in node.ChildNodes)
            ComposeNode(child, tf);
    }
	
}
