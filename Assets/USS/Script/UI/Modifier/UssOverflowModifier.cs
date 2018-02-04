using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UssOverflowModifier
{ 
    [UssModifierKey("overflow")]
    public void ApplyOverflow(Text obj, UssValue value)
    {
        var v = value.AsString();

        if (v == "visible")
        {
            obj.horizontalOverflow = HorizontalWrapMode.Overflow;
            obj.verticalOverflow = VerticalWrapMode.Overflow;
        }
        else if (v == "wrap")
            obj.horizontalOverflow = HorizontalWrapMode.Wrap;
        else if (v == "hidden")
            obj.verticalOverflow = VerticalWrapMode.Truncate;
    }

    [UssModifierKey("overflow-x")]
    public void ApplyOverflowX(Text obj, UssValue value)
    {
        var v = value.AsString();

        if (v == "visible")
            obj.horizontalOverflow = HorizontalWrapMode.Overflow;
        else if (v == "wrap")
            obj.horizontalOverflow = HorizontalWrapMode.Wrap;
    }
    [UssModifierKey("overflow-y")]
    public void ApplyOverflowY(Text obj, UssValue value)
    {
        var v = value.AsString();

        if (v == "visible")
            obj.verticalOverflow = VerticalWrapMode.Overflow;
        else if (v == "hidden")
            obj.verticalOverflow = VerticalWrapMode.Truncate;
    }
}
