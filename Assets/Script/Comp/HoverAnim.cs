using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverAnim : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localEulerAngles = Vector3.zero;

        StopAllCoroutines();
        StartCoroutine(ScaleToFunc(1.2333f));
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleToFunc(originalScale.x));
        StartCoroutine(RotateFunc());
    }

    IEnumerator ScaleToFunc(float to)
    {
        for(int i = 0; i < 30; i++)
        {
            transform.localScale += (Vector3.one * to - transform.localScale) * 0.26f;
            yield return null;
        }
    }
    IEnumerator RotateFunc()
    {
        for (int i = 0; i < 40; i++)
        {
            transform.localEulerAngles = new Vector3(0, 0,
                Mathf.Sin(3.14f / 180 * i * 18) * (40 - i));
            yield return null;
        }
    }
}
 