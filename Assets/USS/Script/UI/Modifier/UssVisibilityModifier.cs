using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UssVisibilityModifier : MonoBehaviour
{
    [UssModifierKey("visibility")]
    public void ApplyVisibility(Graphic g, UssValue value)
    {
        var keywd = value.AsString();

        if (keywd == "visible")
            g.enabled = true;
        else
            g.enabled = false;
    }
}
