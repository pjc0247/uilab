using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UssShadowModifier
{
    [UssModifierKey("shadow")]
    public void Apply(Graphic g, UssValue[] values)
    {
        if (values[0].IsNone())
        {
            GameObject.DestroyImmediate(g.GetComponent<Shadow>());
            return;
        }

        if (values.Length == 1)
        {
            var color = values[0].AsColor();
            var shadow = g.GetComponent<Shadow>();
            if (shadow == null)
                shadow = g.gameObject.AddComponent<Shadow>();
            shadow.effectColor = color;
        }
        else if (values.Length == 2)
        {
            var xy = values[0].AsFloat();
            var color = values[1].AsColor();
            var shadow = g.GetComponent<Shadow>();
            if (shadow == null)
                shadow = g.gameObject.AddComponent<Shadow>();
            shadow.effectDistance = new Vector2(xy, xy);
            shadow.effectColor = color;
        }
        else if (values.Length == 3)
        {
            var x = values[0].AsFloat();
            var y = values[1].AsFloat();
            var color = values[2].AsColor();
            var shadow = g.GetComponent<Shadow>();
            if (shadow == null)
                shadow = g.gameObject.AddComponent<Shadow>();
            shadow.effectDistance = new Vector2(x, y);
            shadow.effectColor = color;
        }
    }
}
