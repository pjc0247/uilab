using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LaunchApp : MonoBehaviour,
    IPointerClickHandler
{
    public RectTransform app;

    public void OnPointerClick(PointerEventData eventData)
    {
        StartCoroutine(LaunchFunc());
    }

    IEnumerator LaunchFunc()
    {
        app.gameObject.SetActive(true);
        app.anchoredPosition = Vector2.zero;
        app.localScale = Vector3.zero;
        for (int i = 0; i < 25; i++)
        {
            app.localScale += (Vector3.one - app.localScale) * 0.25f;
            yield return null;
        }
        app.localScale = Vector3.one;
    }
}
