using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlideToExit : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler
{
    public Image blurLayer;

    private RectTransform app;
    private Vector3 downPosition;
    private Vector2 originalPosition;
    private Vector3 originalScale;

    private Material blurMat;

    void Awake()
    {
        app = transform.parent.GetComponent<RectTransform>();

        originalPosition = app.anchoredPosition;
        originalScale = app.localScale;

        blurMat = new Material(blurLayer.material);
        blurLayer.material = blurMat;
    }
    void Start()
    {
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        downPosition = Input.mousePosition;

        StartCoroutine(SlideFunc());
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        StopAllCoroutines();

        if (Input.mousePosition.y - downPosition.y >= 80)
            StartCoroutine(ExitFunc());
        else
            StartCoroutine(ReturnFunc());
    }

    IEnumerator ReturnFunc()
    {
        for (int i = 0; i < 30; i++)
        {
            app.anchoredPosition +=
                (new Vector2(app.anchoredPosition.x, 0) - app.anchoredPosition)
                * 0.15f;
            app.localScale += (Vector3.one - app.localScale) * 0.15f;
            yield return null;
        }
    }
    IEnumerator SlideFunc()
    {
        while (true)
        {
            var diff = downPosition - Input.mousePosition;
            var offsetY = Mathf.Clamp(-diff.y, 0, 1500);
            var scaleDown = Mathf.Clamp(diff.y / 1024, -0.7f, 0);

            app.anchoredPosition = new Vector2(app.anchoredPosition.x, offsetY);
            app.localScale = Vector3.one * (originalScale.x + scaleDown * originalScale.x);

            blurMat.SetFloat("_BlurSize", Mathf.Clamp(20 + (diff.y / 512), 15, 20));

            yield return null;
        }
    }
    IEnumerator ExitFunc()
    {
        var blur = blurMat.GetFloat("_BlurSize");

        FadeOpacity(0.3f, 0);
        for (int i = 0; i < 30; i++)
        {
            app.localScale += (Vector3.zero - app.localScale) * 0.15f;
            app.anchoredPosition += (new Vector2(0, 1700) - app.anchoredPosition) * 0.15f;

            blur += (0 - blur) * 0.15f;
            blurMat.SetFloat("_BlurSize", blur);

            yield return null;
        }

        app.gameObject.SetActive(false);
    }

    private void FadeOpacity(float duration, float f)
    {
        foreach (var c in app.GetComponentsInChildren<Graphic>())
            c.CrossFadeAlpha(f, duration, true);
    }
}
