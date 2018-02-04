using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UssColorModifier
{
    [UssModifierKey("color")]
    public void ApplyColor(Graphic g, UssValue value)
    {
        var color = value.AsColor();
        g.color = color;
    }

    [UssModifierKey("opacity")]
    public void ApplyOpacity(Graphic g, UssValue value)
    {
        var alpha = value.AsFloat();
        g.canvasRenderer.SetAlpha(alpha);
    }
}
