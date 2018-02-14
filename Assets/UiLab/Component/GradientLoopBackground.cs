using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GradientLoopBackground : UiBase
{
    public Color[] colors = new Color[] { Color.red, Color.green, Color.blue };
    public int frame = 80;

    void Start()
    {
        Invoke("DoFadeIn", 3);
    }

    public void DoFadeIn()
    {
        StartFadeAnimation(FadeInFunc(frame));
    }
    IEnumerator FadeInFunc(int frame)
    {
        if (colors == null || colors.Length < 2)
            yield break;

        var cursor = 0;
        while (true)
        {
            var f = 0.0f;

            var a = colors.Length + ((cursor) % colors.Length);
            if ((cursor) % colors.Length == 0) a = 0;
            var b = colors.Length + ((cursor - 2) % colors.Length);
            if ((cursor-2) % colors.Length == 0) b = 0;

            uiMaterial.SetColor("_Color", colors[a]);
            uiMaterial.SetColor("_Color2", colors[b]);
            uiMaterial.SetFloat("_Offset", 0);
            for (int i = 0; i <= frame; i++)
            {
                var t = 1.0f / frame * i;
                f = Easing.QuadOut(t);
                uiMaterial.SetFloat("_Offset", f);
                yield return null;
            }

            cursor--;
        }
    }
}
