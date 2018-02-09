using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeColor : ScrollReactive
{
    public int targetY;
    public Color targetColor;
    public Image img;

    public void OnScroll(int y)
    {
        if (y <= targetY - 1000) return;

        var blend = Mathf.Clamp((float)(targetY - y) / 500, 0, 1);
        img.color = Color.white * blend + (targetColor * (1 - blend));
        img.color += new Color(0, 0, 0, 1);
    }
}
