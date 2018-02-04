using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UssPaddingModifier
{
    [UssModifierKey("padding-left")]
    public void ApplyPaddingLeft(RectTransform g, UssValue value)
    {
        var v = value.AsFloat();
        g.offsetMin = new Vector2(v, g.offsetMin.y);
    }
    [UssModifierKey("padding-right")]
    public void ApplyPaddingRight(RectTransform g, UssValue value)
    {
        var v = value.AsFloat();
        g.offsetMax = new Vector2(-v, g.offsetMax.y);
    }
    [UssModifierKey("padding-bottom")]
    public void ApplyPaddingBottom(RectTransform g, UssValue value)
    {
        var v = value.AsFloat();
        g.offsetMin = new Vector2(g.offsetMin.x, v);
    }
    [UssModifierKey("padding-top")]
    public void ApplyPaddingTop(RectTransform g, UssValue value)
    {
        var v = value.AsFloat();
        g.offsetMax = new Vector2(g.offsetMin.x, -v);
    }
}
