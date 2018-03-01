using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeInWithDrop : UiBase
{
    void Start ()
    {
        Invoke("DoFadeIn", 1.5f);
	}
    public void DoFadeIn()
    {
        var canvasSize = GetCanvasSize();

        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, canvasSize.y + 1000);

        StartCoroutine(FadeInFunc());
    }
    IEnumerator FadeInFunc()
    {
        MoveTo(25, originalPosition, Easing.BackOut);

        yield return StartFadeAnimation(RotateToFunc(rt, 20, -30, Easing.SineOut));
        yield return StartFadeAnimation(RotateToFunc(rt, 18, 0, Easing.SineOut));
    }
}
