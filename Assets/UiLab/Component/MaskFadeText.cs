using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaskFadeText : UiBase
{
    public RectTransform leftText, rightText;

	void Start ()
    {
        Invoke("DoFadeOut", 3);
	}

    public void DoFadeOut()
    {
        MoveTo(leftText, 27, new Vector2(500, 0), Easing.QuadIn);
        MoveTo(rightText, 27, new Vector2(-500, 0), Easing.QuadIn);
    }
}
