using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GradientBackground : UiBase
{
    public int frame = 80;

	void Start ()
    {
        Invoke("DoFadeIn", 3);
	}

    public void DoFadeIn()
    {
        StartFadeAnimation(FadeInFunc(frame));
    }
    IEnumerator FadeInFunc(int frame)
    {
        var f = 0.0f;

        uiMaterial.SetFloat("_Offset", 0);
        for (int i = 0; i <= frame; i++)
        {
            var t = 1.0f / frame * i;
            f = Easing.QuadOut(t);
            uiMaterial.SetFloat("_Offset", f);
            yield return null;
        }
    }
}
