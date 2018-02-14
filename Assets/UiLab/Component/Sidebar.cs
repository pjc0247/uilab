using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sidebar : UiBase
{
    public int width;
    public Color dim;

    public RectTransform dimArea;
    public RectTransform contentToPull;

    protected override void Awake()
    {
        base.Awake();

        dimArea.SetAlpha(0);
    }
    void Start ()
    {
	}
    void LateUpdate()
    {
        if (contentToPull == null) return;
        if (IsPlaying("move") == false) return;

        contentToPull.anchoredPosition = new Vector2(
            (positionX + rt.rect.width) * rt.localScale.x, contentToPull.anchoredPosition.y);
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
        MoveTo(17, new Vector2(0, rt.anchoredPosition.y), Easing.BackOut);
        dimArea.AlphaTo(1, 0.12f);
        dimArea.GetComponent<Image>().raycastTarget = true;
    }
    public void Hide()
    {
        MoveTo(17, new Vector2(-width, rt.anchoredPosition.y), Easing.BackIn);
        dimArea.AlphaTo(0, 0.16f);
        dimArea.GetComponent<Image>().raycastTarget = false;
    }
}
