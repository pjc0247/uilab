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
        var elem = g.GetComponent<UtmlElement>();
        elem.padding.x = v;
    }
    [UssModifierKey("padding-right")]
    public void ApplyPaddingRight(RectTransform g, UssValue value)
    {
        var v = value.AsFloat();
        var elem = g.GetComponent<UtmlElement>();
        elem.padding.width = v;
    }
    [UssModifierKey("padding-bottom")]
    public void ApplyPaddingBottom(RectTransform g, UssValue value)
    {
        var v = value.AsFloat();
        var elem = g.GetComponent<UtmlElement>();
        elem.padding.height = v;
    }
    [UssModifierKey("padding-top")]
    public void ApplyPaddingTop(RectTransform g, UssValue value)
    {
        var v = value.AsFloat();
        var elem = g.GetComponent<UtmlElement>();
        elem.padding.y = v;
    }
}
