using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfilePage : MonoBehaviour
{
    public Image scrollIndicator;

    public RectTransform devSpecs;
    public ScrollRect scroll;

    public RectTransform profileImage;

    private Vector2 originalProfileImagePosition;
    private Dictionary<RectTransform, int> devSpecsTiming;
    private Dictionary<RectTransform, Vector2> devSpecsOriginalPositions;

    void Start ()
    {
        originalProfileImagePosition = profileImage.anchoredPosition;

        devSpecsTiming = new Dictionary<RectTransform, int>();
        devSpecsOriginalPositions = new Dictionary<RectTransform, Vector2>();
        for (int i = 0; i < devSpecs.childCount; i++)
        {
            var c = devSpecs.GetChild(i).GetComponent<RectTransform>();
            devSpecsOriginalPositions[c] = c.anchoredPosition;
            devSpecsTiming[c] = Random.Range(0, 2000);
            c.anchoredPosition = Vector2.zero;
            c.localScale = Vector3.zero;
        }
    }

    public void OnScrollValueChanged(Vector2 v)
    {
        var y = scroll.content.anchoredPosition.y;

        //Debug.Log(y);

        foreach (var c in GetComponentsInChildren<ScrollReactive>())
            c.SendMessage("OnScroll", y);

        if (y <= 1050)
            scrollIndicator.canvasRenderer.SetAlpha(1 - (y / 200));
        if (y <= 3350)
        {
            profileImage.anchoredPosition = originalProfileImagePosition - new Vector2(0, y/(1.0f));
        }
        foreach (var p in devSpecsTiming)
        {
            if (y <= p.Value)
            {
                p.Key.anchoredPosition = Vector2.zero;
                p.Key.localScale = Vector2.zero;
            }
            else
            {
                p.Key.anchoredPosition = devSpecsOriginalPositions[p.Key] * BounceOut((y - p.Value) / (3300 - p.Value));
                p.Key.localScale = Vector3.one * BounceOut((y - p.Value) / (3300 - p.Value));
            }
        }
    }

    float BounceOut(float t)
    {
        t = Mathf.Clamp(t, 0, 1);

        if (t < (1.0f / 2.75f))
            return 7.5625f * t * t;
        if (t < (2.0f / 2.75f))
        {
            t -= 1.5f / 2.75f;
            return 7.5625f * t * t + 0.75f;
        }
        if (t < (2.5f / 2.75f))
        {
            t -= 2.25f / 2.75f;
            return 7.5625f * t * t + 0.9375f;
        }
        t -= 2.625f / 2.75f;
        return 7.5625f * t * t + 0.984375f;
    }
}
