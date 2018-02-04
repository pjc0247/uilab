using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfilePage : MonoBehaviour
{
    public RectTransform devSpecs;
    public ScrollRect scroll;

    public RectTransform profileImage;

    private Vector2 originalProfileImagePosition;

    void Start () {
        originalProfileImagePosition = profileImage.anchoredPosition;

    }

    public void OnScrollValueChanged(Vector2 v)
    {
        var y = scroll.content.sizeDelta.y - scroll.content.sizeDelta.y * v.y;

        Debug.Log(y);
        if (y <= 2500)
        {
            profileImage.anchoredPosition = originalProfileImagePosition - new Vector2(0, y/4);
        }
        devSpecs.localScale = Vector2.one * (Mathf.Clamp(y / 2500, 0, 1));
    }
}
