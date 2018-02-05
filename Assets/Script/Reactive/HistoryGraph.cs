using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HistoryGraph : ScrollReactive
{
    public RectTransform graphBar;
    public RectTransform ball1, ball2, ball3, ball4;

    private float barMaxHeight;

    void Start()
    {
        barMaxHeight = graphBar.sizeDelta.y;
        graphBar.sizeDelta = new Vector2(graphBar.sizeDelta.x, 0);

        ball1.localScale = Vector3.one * 3;
        SetAlpha(ball1.gameObject, 0);
        ball2.localScale = Vector3.zero;
        ball3.localScale = Vector3.zero;
        ball4.localScale = Vector3.zero;
    }
    public void OnScroll(int y)
    {
        if (y <= 4500) return;
        var dy = y - 4500;

        graphBar.sizeDelta = new Vector2(graphBar.sizeDelta.x, Mathf.Clamp(dy * 1.3f, 0, barMaxHeight));
        graphBar.GetComponent<Image>().canvasRenderer.SetAlpha(Mathf.Clamp((float)(y - 4500) / 222, 0, 1));

        ball1.localScale = Vector3.one * (3 - 2 * (Mathf.Clamp((float)(y - 4500) / 222, 0, 1)));
        SetAlpha(ball1.gameObject, Mathf.Clamp((float)(y - 4500) / 222, 0, 1));
        ball2.localScale = Vector3.one * Mathf.Clamp((float)(y - 4700) / 222, 0, 1);
        ball3.localScale = Vector3.one * Mathf.Clamp((float)(y - 4900) / 222, 0, 1);
        ball4.localScale = Vector3.one * Mathf.Clamp((float)(y - 5100) / 222, 0, 1);
    }

    private void SetAlpha(GameObject g, float f)
    {
        foreach (var c in g.GetComponentsInChildren<Graphic>())
            c.canvasRenderer.SetAlpha(f);
    }
}
