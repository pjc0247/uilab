using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeIn : ScrollReactive
{
    public int targetY = 0;
    public float initialScale = 3;

    private float originalScale;

    void Awake()
    {
        originalScale = transform.localScale.x;
    }
	public void OnScroll(int y)
    {
        if (y <= targetY - 300) return;
        if (y >= targetY + 300) return;

        var value = Mathf.Clamp(((float)y - targetY) / 300, 0, 1);

        GetComponent<Graphic>().canvasRenderer.SetAlpha(value);
        transform.localScale = Vector3.one * initialScale +  Vector3.one * (1 - initialScale) * value;
    }
}

