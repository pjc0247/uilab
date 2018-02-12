using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sidebar : UiBase
{
    public int width;
    public Color dim;

    public RectTransform dimArea;

    protected override void Awake()
    {
        base.Awake();

        dimArea.SetAlpha(0);
    }
    void Start ()
    {
        Invoke("Show", 4);
	}
    void OnValidate()
    {
        var rt = GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(-width, 0);
        rt.sizeDelta = new Vector2(width, rt.sizeDelta.y);

        dimArea.anchoredPosition = new Vector2(width, dimArea.anchoredPosition.y);
        dimArea.sizeDelta = new Vector2(GetCanvasSize().x, dimArea.sizeDelta.y);
        dimArea.GetComponent<Image>().color = dim;
        dimArea.SetAlpha(0);
    }

    public void Show()
    {
        MoveTo(9, new Vector2(0, rt.anchoredPosition.y), Easing.SineOut);
        dimArea.AlphaTo(1, 0.05f);
    }
    public void Hide()
    {
        MoveTo(9, new Vector2(-width, rt.anchoredPosition.y), Easing.SineOut);
        dimArea.AlphaTo(0, 0.05f);
    }
}
