using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UiState))]
public class UiBase : MonoBehaviour
{
    #region SHORTCUTS
    public Vector2 position
    {
        get { return rt.anchoredPosition; }
        set { rt.anchoredPosition = value; }
    }
    public float positionX
    {
        get { return rt.anchoredPosition.x; }
        set { rt.anchoredPosition = new Vector2(value, rt.anchoredPosition.y); }
    }
    public float positionY
    {
        get { return rt.anchoredPosition.y; }
        set { rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, value); }
    }
    public float rotation
    {
        get { return rt.localEulerAngles.z; }
        set { rt.localEulerAngles = new Vector3(0, 0, value); }
    }
    public float scale
    {
        get { return rt.localEulerAngles.magnitude; }
        set { rt.localEulerAngles = new Vector3(value, value, value); }
    }
    #endregion

    protected UiState state;
    [HideInInspector] public RectTransform rt;
    [HideInInspector] public Graphic graphic;

    protected Vector2 originalPosition;
    protected Vector3 originalScale;
    protected Vector2 originalSize;

    private GraphicRaycaster _raycaster;
    protected GraphicRaycaster raycaster
    {
        get
        {
            if (_raycaster == null)
                _raycaster = GetComponentInParent<GraphicRaycaster>();
            return _raycaster;
        }
    }

    private Material _uiMaterial;
    protected Material uiMaterial
    {
        get
        {
            if (graphic == null) return null;
            if (_uiMaterial == null)
                graphic.material = _uiMaterial = new Material(graphic.material);
            return _uiMaterial;
        }
    }

    protected virtual void Awake()
    {
        state = GetComponent<UiState>();
        rt = GetComponent<RectTransform>();
        graphic = GetComponent<Graphic>();

        originalSize = rt.sizeDelta;
        originalPosition = rt.anchoredPosition;
        originalScale = rt.localScale;
    }

    public bool IsPlaying(string key)
    {
        return state.IsPlaying(key);
    }
    public Coroutine StartAnimation(string key, IEnumerator coro)
    {
        return state.StartAnimation(key, coro);
    }
    public void StopAnimation(string key)
    {
        state.ForceStopAnimation(key);
    }
    public void StopAllAnimation()
    {
        state.ForceStopAllAnimations();
    }
    public Coroutine StartFadeAnimation(IEnumerator coro)
    {
        return StartAnimation("fade", coro);
    }
    public void StopFadeAnimation()
    {
        StopAnimation("fade");
    }

    protected Vector2 GetCanvasSize()
    {
        return GetComponentInParent<CanvasScaler>().referenceResolution;
    }

    public void PlayFadeIn()
    {
        SendMessage("DoFadeIn", SendMessageOptions.DontRequireReceiver);
    }
    public void PlayFadeOut()
    {
        SendMessage("DoFadeOut", SendMessageOptions.DontRequireReceiver);
    }
    public void PlayHaptic()
    {
        SendMessage("DoHaptic", SendMessageOptions.DontRequireReceiver);
    }

    public Coroutine MoveTo(int frame, Vector2 target, Easing.EasingDelegate func)
    {
        return StartAnimation("move", MoveToFunc(rt, frame, target, func));
    }
    public Coroutine MoveTo(RectTransform tr, int frame, Vector2 target, Easing.EasingDelegate func)
    {
        return StartAnimation("move", MoveToFunc(tr, frame, target, func));
    }
    public Coroutine MoveTo(Graphic g, int frame, Vector2 target, Easing.EasingDelegate func)
    {
        var rt = g.GetComponent<RectTransform>();
        if (rt == null) throw new ArgumentException("g");
        return StartAnimation("move", MoveToFunc(rt, frame, target, func));
    }
    public Coroutine MoveBy(int frame, Vector2 target, Easing.EasingDelegate func)
    {
        return MoveTo(rt, frame, rt.anchoredPosition + target, func);
    }
    public Coroutine MoveBy(RectTransform tr, int frame, Vector2 target, Easing.EasingDelegate func)
    {
        return MoveTo(tr, frame, tr.anchoredPosition + target, func);
    }
    public Coroutine MoveBy(Graphic g, int frame, Vector2 target, Easing.EasingDelegate func)
    {
        var rt = g.GetComponent<RectTransform>();
        if (rt == null) throw new ArgumentException("g");
        return MoveBy(rt, frame, target, func);
    }
    protected IEnumerator MoveToFunc(RectTransform tr, int frame, Vector2 target, Easing.EasingDelegate func)
    {
        var origin = tr.anchoredPosition;
        var diff = target - origin;

        for (int i = 0; i <= frame; i++)
        {
            var t = 1.0f / frame * i;

            tr.anchoredPosition = origin + func(t) * diff;
            yield return null;
        }
    }

    public Coroutine SizeTo(int frame, Vector2 target, Easing.EasingDelegate func)
    {
        return StartCoroutine(SizeToFunc(rt, frame, target, func));
    }
    public Coroutine SizeTo(RectTransform tr, int frame, Vector2 target, Easing.EasingDelegate func)
    {
        return StartCoroutine(SizeToFunc(tr, frame, target, func));
    }
    protected IEnumerator SizeToFunc(RectTransform tr, int frame, Vector2 target, Easing.EasingDelegate func)
    {
        var origin = tr.sizeDelta;
        var diff = target - origin;

        for (int i = 0; i <= frame; i++)
        {
            var t = 1.0f / frame * i;
            tr.sizeDelta = origin + func(t) * diff;
            yield return null;
        }
    }

    public Coroutine ScaleTo(int frame, Vector3 target, Easing.EasingDelegate func)
    {
        return StartAnimation("scale", ScaleToFunc(rt, frame, target, func));
    }
    public Coroutine ScaleTo(RectTransform tr, int frame, Vector3 target, Easing.EasingDelegate func)
    {
        return StartAnimation("scale", ScaleToFunc(tr, frame, target, func));
    }
    protected IEnumerator ScaleToFunc(RectTransform tr, int frame, Vector3 target, Easing.EasingDelegate func)
    {
        var origin = tr.localScale;
        var diff = target - origin;

        for (int i = 0; i <= frame; i++)
        {
            var t = 1.0f / frame * i;
            tr.localScale = origin + func(t) * diff;
            yield return null;
        }
    }

    public Coroutine RotateTo(int frame, float angle, Easing.EasingDelegate func)
    {
        return StartCoroutine(RotateToFunc(frame, angle, func));
    }
    protected IEnumerator RotateToFunc(int frame, float target, Easing.EasingDelegate func)
    {
        var origin = rt.localEulerAngles.z;
        if (origin > 180) origin = origin - 360;
        var diff = target - origin;

        for (int i = 0; i <= frame; i++)
        {
            var t = 1.0f / frame * i;
            rt.localEulerAngles = new Vector3(0, 0, origin + func(t) * diff);
            yield return null;
        }
    }
}
