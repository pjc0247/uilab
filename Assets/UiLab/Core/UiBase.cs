using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiBase : MonoBehaviour
{
    protected RectTransform rt;
    protected Graphic graphic;

    protected Vector2 originalPosition;
    protected Vector3 originalScale;

    protected float alpha
    {
        get { return graphic.canvasRenderer.GetAlpha(); }
        set { graphic.canvasRenderer.SetAlpha(value); }
    }

    protected virtual void Awake()
    {
        rt = GetComponent<RectTransform>();
        graphic = GetComponent<Graphic>();

        originalPosition = rt.anchoredPosition;
        originalScale = rt.localScale;
    }
	void Start ()
    {
		
	}
    
    protected Vector2 GetCanvasSize()
    {
        return GetComponentInParent<CanvasScaler>().referenceResolution;
    }

    protected void MoveTo(int frame, Vector2 target, Easing.EasingDelegate func)
    {
        StartCoroutine(MoveToFunc(frame, target, func));
    }
    protected IEnumerator MoveToFunc(int frame, Vector2 target, Easing.EasingDelegate func)
    {
        var origin = rt.anchoredPosition;
        var diff = target - origin;

        for (int i = 0; i <= frame; i++)
        {
            var t = 1.0f / frame * i;
            rt.anchoredPosition = origin + func(t) * diff;
            yield return null;
        }
    }

    protected void ScaleTo(int frame, Vector3 target, Easing.EasingDelegate func)
    {
        StartCoroutine(ScaleToFunc(frame, target, func));
    }
    protected IEnumerator ScaleToFunc(int frame, Vector3 target, Easing.EasingDelegate func)
    {
        var origin = rt.localScale;
        var diff = target - origin;

        for (int i = 0; i <= frame; i++)
        {
            var t = 1.0f / frame * i;
            rt.localScale = origin + func(t) * diff;
            yield return null;
        }
    }

    protected void RotateTo(int frame, float angle, Easing.EasingDelegate func)
    {
        StartCoroutine(RotateToFunc(frame, angle, func));
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
