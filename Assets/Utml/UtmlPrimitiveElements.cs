using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[UtmlElementName("vertical")]
[ExecuteInEditMode]
public class UtmlVerticalElement : UtmlElement
{
    public override void Construct(UtmlConstructionData cd)
    {
    }
    public override void PostConstruct(UtmlConstructionData cd)
    {
    }

    void Update()
    {
        Layout();
    }

    protected void Layout()
    {
        var offset = 0.0f;

        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i).GetComponent<RectTransform>();
            var elem = child.GetComponent<UtmlElement>();

            offset += elem.padding.y;
            offset += child.sizeDelta.y * child.pivot.y;

            var offsetX = child.sizeDelta.x * child.pivot.x;

            child.anchorMin = new Vector2(0, 1);
            child.anchorMax = new Vector2(0, 1);
            child.anchoredPosition = new Vector2(offsetX + elem.padding.x, -offset);

            offset += child.sizeDelta.y * (1 - child.pivot.y) + elem.padding.height;
        }
    }
}
[UtmlElementName("horizontal")]
[ExecuteInEditMode]
public class UtmlHorizontalElement : UtmlElement
{
    public override void Construct(UtmlConstructionData cd)
    {
        //var v = gameObject.AddComponent<HorizontalLayoutGroup>();
        //v.childForceExpandWidth = false;
    }
    public override void PostConstruct(UtmlConstructionData cd)
    {
            
    }

    void Update()
    {
        Layout();
    }

    protected void Layout()
    {
        var offset = 0.0f;

        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i).GetComponent<RectTransform>();
            var elem = child.GetComponent<UtmlElement>();

            offset += elem.padding.x;
            offset += child.sizeDelta.x * child.pivot.x;

            var offsetY = child.sizeDelta.y * child.pivot.y;

            child.anchorMin = new Vector2(0 ,1);
            child.anchorMax = new Vector2(0, 1);
            child.anchoredPosition = new Vector2(offset, -offsetY -elem.padding.y);

            offset += child.sizeDelta.x * (1 - child.pivot.x) + elem.padding.width;
        }
    }
}

[UtmlElementName("text")]
public class UtmlTextElement : UtmlElement
{
    protected Text text;

    public override void Construct(UtmlConstructionData cd)
    {
        text = gameObject.AddComponent<Text>();
        text.text = cd.GetText();

        var rt = text.GetComponent<RectTransform>();
        rt.sizeDelta = GetContentSize(cd.GetText());
    }

    protected Vector2 GetContentSize(string content)
    {
        var textGen = new TextGenerator();
        var generationSettings = text.GetGenerationSettings(text.rectTransform.rect.size);
        var width = textGen.GetPreferredWidth(content, generationSettings);
        var height = textGen.GetPreferredHeight(content, generationSettings);

        return new Vector2(width, height);
    }
}

[UtmlElementName("img")]
public class UtmlImgElement : UtmlElement
{
    public override void Construct(UtmlConstructionData cd)
    {
        var img = gameObject.AddComponent<Image>();

        img.sprite = UtmlAssetLoader.Load<Sprite>(cd.GetStringAttribute("src", ""));
    }
}