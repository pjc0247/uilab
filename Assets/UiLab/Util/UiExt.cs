using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class UiExt
{
    public static void AlphaTo(this Component c, float v, float duration)
    {
        c.GetComponent<Graphic>().CrossFadeAlpha(v, duration, true);
    }

    public static void SetAlpha(this Component c, float v)
    {
        c.GetComponent<Graphic>().canvasRenderer.SetAlpha(v);
    }
    public static float GetAlpha(this Component c)
    {
        return c.GetComponent<Graphic>().canvasRenderer.GetAlpha();
    }
    public static void SetAlpha(this Graphic g, float v)
    {
        g.canvasRenderer.SetAlpha(v);
    }
    public static float GetAlpha(this Graphic g)
    {
        return g.canvasRenderer.GetAlpha();
    }
}
