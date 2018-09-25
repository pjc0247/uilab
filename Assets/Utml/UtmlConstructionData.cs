using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HtmlAgilityPack;

public class UtmlConstructionData
{
    public string name;
    public Dictionary<string, string> attributes;
    public string innerText;

    private HtmlNode node;

    public UtmlConstructionData(HtmlNode _node)
    {
        node = _node;

        innerText = node.InnerText;
        name = node.Name;
        attributes = node.Attributes.ToDictionary(x => x.Name, x => x.Value);
    }

    public string GetText()
    {
        var strs = node.ChildNodes
            .Where(x => x.NodeType == HtmlNodeType.Text)
            .Select(x => x.InnerText)
            .ToArray();
        return string.Join("", strs);
    }

    public bool HasAttribute(string key)
    {
        if (attributes.ContainsKey(key))
            return true;
        return false;
    }
    public string GetStringAttribute(string key, string @default)
    {
        if (attributes.ContainsKey(key))
            return (string)attributes[key];
        return @default;
    }
}