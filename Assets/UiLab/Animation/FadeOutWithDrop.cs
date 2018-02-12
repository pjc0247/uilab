using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOutWithDrop : UiBase
{
    private Vector2 origin;

    void Start()
    {
        Invoke("DoFadeOut", 3.5f);
    }
    public void DoFadeOut()
    {
        var canvasSize = GetCanvasSize();

        origin = rt.anchoredPosition;
        StartCoroutine(FadeOutFunc());
    }
    IEnumerator FadeOutFunc()
    {
        MoveTo(25, originalPosition - new Vector2(0, 1000), Easing.BackIn);
        yield return new WaitForSeconds(0.05f);
        RotateTo(20, -45, Easing.SineOut);

        yield return new WaitForSeconds(1);

        gameObject.SetActive(false);
    }
}
