using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollDown : MonoBehaviour
{
    private RectTransform rt;
    private Vector2 originalPosition;

    private int f = 0;

	void Start () {
        rt = GetComponent<RectTransform>();
        originalPosition = rt.anchoredPosition;
	}
	void Update () {
        rt.anchoredPosition = originalPosition + new Vector2(0, f);

        f-= 2;
        if (f <= -50) f = 0;
	}
}
