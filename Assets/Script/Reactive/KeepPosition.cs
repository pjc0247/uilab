using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepPosition : ScrollReactive
{
    public int targetY;

    private RectTransform rt;
    private Vector2 originalPosition;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        originalPosition = rt.anchoredPosition;
    }

	public void OnScroll(int y)
    {
        if (y <= targetY - 400) return;

        rt.anchoredPosition = originalPosition - new Vector2(0, y - targetY);
    }
}
