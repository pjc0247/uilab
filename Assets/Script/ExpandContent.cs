using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpandContent : MonoBehaviour
{
    public Image headImage;
    public RectTransform content;

    public RectTransform headImageCullLayer, contentTopMargin;

    private RectTransform rt;
    private Vector3 originalHeadImageScale;
    private Vector2 originalPosition;
    private Vector2 originalSizeDelta, 
        originalHeadImageCullLayerSizeDelta, originalContentTopMarginSizeDelta;
    private bool expanded = false;

    void Awake()
    {
        rt = GetComponent<RectTransform>();

        originalHeadImageScale = headImage.transform.localScale;
        originalPosition = rt.anchoredPosition;
        originalSizeDelta = rt.sizeDelta;

        originalHeadImageCullLayerSizeDelta = headImageCullLayer.sizeDelta;
        originalContentTopMarginSizeDelta = contentTopMargin.sizeDelta;
    }

    public void OnExpand() 
    {
        expanded = true;

        StartCoroutine(ExpandFunc());
        StartCoroutine(ExpandScaleFunc());

        SendMessage("OnExpandContent");
        foreach (var g in GetComponentsInChildren<Content>())
            g.SendMessage("OnExpandContent");
    }
    public void OnShrink()
    {
        expanded = false;

        StartCoroutine(ShrinkFunc());

        SendMessage("OnShrinkContent");
        foreach (var g in GetComponentsInChildren<Content>())
            g.SendMessage("OnShrinkContent");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            OnShrink();
    }

    IEnumerator ExpandFunc()
    {
        rt.SetAsLastSibling();

        for (int i = 0; i < 30; i++)
        {
            headImage.transform.localScale += (Vector3.one - headImage.transform.localScale) * 0.25f;

            headImageCullLayer.sizeDelta += (new Vector2(headImageCullLayer.sizeDelta.x, 1000) - headImageCullLayer.sizeDelta) * 0.25f;
            contentTopMargin.sizeDelta += (new Vector2(contentTopMargin.sizeDelta.x, 1000) - contentTopMargin.sizeDelta) * 0.25f;

            rt.anchoredPosition += (new Vector2(0, 160) - rt.anchoredPosition) * 0.25f;
            rt.sizeDelta += (new Vector2(rt.sizeDelta.x, 3000) - rt.sizeDelta) * 0.25f;

            yield return null;
        }
    }
    IEnumerator ExpandScaleFunc()
    {
        var start = transform.localScale;
        var startContent = content.localScale;
        var diff = (Vector3.one * 1.2f - transform.localScale);
        var diffContent = (Vector3.one * 0.925f) - content.localScale;
        
        for (int i = 0; i < 25; i++)
        {
            var c = (float)i / 25;

            content.localScale = startContent + SineOut(c) * diffContent;
            transform.localScale = new Vector3(
                (start + SineOut(Mathf.Clamp(c * 2, 0, 1)) * diff).x,
                (start + SineOut(c) * diff).y,
                1);

            yield return null;
        }
    }
    float SineOut(float t)
    {
        return Mathf.Clamp(Mathf.Sin(t * 3.14f / 2), 0, 1);
    }

    IEnumerator ShrinkFunc()
    {
        for (int i = 0; i < 30; i++)
        {
            headImage.transform.localScale += (originalHeadImageScale - headImage.transform.localScale) * 0.25f;

            headImageCullLayer.sizeDelta += (originalHeadImageCullLayerSizeDelta - headImageCullLayer.sizeDelta) * 0.25f;
            contentTopMargin.sizeDelta += (originalContentTopMarginSizeDelta - contentTopMargin.sizeDelta) * 0.25f;

            rt.anchoredPosition += (originalPosition - rt.anchoredPosition) * 0.25f;
            transform.localScale += (Vector3.one - transform.localScale) * 0.25f;
            content.localScale += (Vector3.one - content.localScale) * 0.25f;
            rt.sizeDelta += (originalSizeDelta - rt.sizeDelta) * 0.25f;

            yield return null;
        }
    }
}
