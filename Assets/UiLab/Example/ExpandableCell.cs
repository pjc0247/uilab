﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ExpandableCell : UiBase
{
    public RectTransform mainTexture;
    public RectTransform topDeco;

    private bool isExpanded = false;
    private Vector3 originalMainTextureScale;
    private KeepScale mainTextureKeepScale;

    protected override void Awake()
    {
        base.Awake();

        mainTextureKeepScale = mainTexture.GetComponent<KeepScale>();
        originalMainTextureScale = mainTexture.localScale;
    }
    void Start()
    {

    }

    void OnClickCell()
    {
        isExpanded ^= true;
        if (isExpanded) Expand();
        else Shrink();
    }

    public void Expand()
    {
        mainTextureKeepScale.enabled = false;

        MoveTo(topDeco, 20, new Vector2(-130, 300), Easing.BackOut);
        ScaleTo(mainTexture, 20, Vector3.one, Easing.SineOut);
        SizeTo(20, new Vector2(rt.sizeDelta.x, 1060), Easing.BackOut);
    }
    public void Shrink()
    {
        mainTextureKeepScale.enabled = true;

        MoveTo(topDeco, 17, new Vector2(0, 0), Easing.SineOut);
        ScaleTo(mainTexture, 20, originalMainTextureScale, Easing.SineIn);
        SizeTo(17, originalSize, Easing.SineOut);
    }
}