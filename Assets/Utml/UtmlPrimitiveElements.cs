using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[UtmlElementName("text")]
public class UtmlTextElement : UtmlElement
{
    public static void Construct(GameObject go, UtmlConstructionData cd)
    {
        var text = go.AddComponent<Text>();
        text.text = cd.GetText();
    }
}

[UtmlElementName("img")]
public class UtmlImgElement : UtmlElement
{
    public static void Construct(GameObject go, UtmlConstructionData cd)
    {
        var img = go.AddComponent<Image>();
        img.sprite = Resources.Load<Sprite>(cd.GetStringAttribute("src", ""));
    }
}