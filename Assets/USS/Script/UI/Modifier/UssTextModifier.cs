using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UssTextModifier
{
    [UssModifierKey("font-size")]
    public void ApplyFontSize(Text g, UssValue value)
    {
        g.fontSize = value.AsInt();
    }
    [UssModifierKey("font-style")]
    public void ApplyFontStyle(Text g, UssValue value)
    {
        var style = value.AsString();

        if (style == "normal")
            g.fontStyle = FontStyle.Normal;
        else if (style == "bold")
            g.fontStyle = FontStyle.Bold;
        else if (style == "italic")
            g.fontStyle = FontStyle.Italic;
    }

    [UssModifierKey("font-family")]
    public void ApplyFontFamilty(Text g, UssValue value)
    {
    }

    [UssModifierKey("text-align")]
    public void ApplyTextAlign(Text g, UssValue[] values)
    {
        var alignH = values[0].AsString();
        if (values.Length == 1)
        {
            if (alignH == "left")
                g.alignment = TextAnchor.MiddleLeft;
            else if (alignH == "center")
                g.alignment = TextAnchor.MiddleCenter;
            else if (alignH == "right")
                g.alignment = TextAnchor.MiddleRight;
            else
                throw new ArgumentException("Invalid param: " + alignH);
        }
        else
        {
            var alignV = values[1].AsString();

            if (alignH == "left" && alignV == "top")
                g.alignment = TextAnchor.UpperLeft;
            else if (alignH == "center" && alignV == "top")
                g.alignment = TextAnchor.UpperCenter;
            else if (alignH == "right" && alignV == "top")
                g.alignment = TextAnchor.UpperRight;

            else if (alignH == "left" && alignV == "middle")
                g.alignment = TextAnchor.MiddleLeft;
            else if (alignH == "center" && alignV == "middle")
                g.alignment = TextAnchor.MiddleCenter;
            else if (alignH == "right" && alignV == "middle")
                g.alignment = TextAnchor.MiddleRight;

            else if (alignH == "left" && alignV == "bottom")
                g.alignment = TextAnchor.LowerLeft;
            else if (alignH == "center" && alignV == "bottom")
                g.alignment = TextAnchor.LowerCenter;
            else if (alignH == "right" && alignV == "bottom")
                g.alignment = TextAnchor.LowerRight;

            else
                throw new ArgumentException("Invalid param: " + alignH + ", " + alignV);
        }
    }
}
