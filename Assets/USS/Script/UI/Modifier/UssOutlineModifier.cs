using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UssOutlineModifier
{
    [UssModifierKey("outline")]
    public void Apply(Graphic g, UssValue[] values)
    {
        if (values[0].IsNone())
        {
            GameObject.DestroyImmediate(g.GetComponent<Outline>());
            return;
        }

        if (values.Length == 1)
        {
            var color = values[0].AsColor();
            var outline = g.GetComponent<Outline>();
            if (outline == null)
                outline = g.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
        }
        else if (values.Length == 2)
        {
            var width = values[0].AsFloat();
            var color = values[1].AsColor();
            var outline = g.GetComponent<Outline>();
            if (outline == null)
                outline = g.gameObject.AddComponent<Outline>();
            outline.effectDistance = new Vector2(width, width);
            outline.effectColor = color;
        }
    }
}
