using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Parallax : UiBase
{
    public float speedFactor = 0.25f;

    public RectTransform background;
    public Texture2D texture;

	void Start ()
    {
		
	}
    void Update()
    {
        if (background == null) return;

        var t = ((Mathf.Cos(Time.time * speedFactor) + 1) / 2) * -(background.sizeDelta.x - rt.rect.width);
        background.anchoredPosition = new Vector2(t, rt.anchoredPosition.y);
    }

    void OnValidate()
    {
        if (background == null) return;

        background.anchoredPosition = new Vector2(0, background.anchoredPosition.y);
        background.GetComponent<RawImage>().texture = texture;
        background.GetComponent<AspectRatioFitter>().aspectRatio = (float)texture.width / texture.height;
    }	
}
