using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SwipeToShrink : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    public ScrollRect scroll;

    public bool isPointerEntered = false;
    public bool isExpaneded = false;
    private Vector3 downPosition;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerEntered = true;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerEntered = false;
    }

    public void OnExpandContent()
    {
        isExpaneded = true;   

        if (isPointerEntered)
            StartCoroutine(SwipeFunc());
    }
    public void OnShirnkContent()
    {
        isExpaneded = false;

        StopAllCoroutines();
    }

    IEnumerator SwipeFunc()
    {
        while (true)
        {
            var ignorePhase = 5;
                
            while (Input.GetMouseButton(0) == false)
                yield return null;
            downPosition = Input.mousePosition;
            while (Input.GetMouseButton(0))
            {
                var delta = Input.mousePosition - downPosition;
                if (scroll.normalizedPosition.y <= 0.98f)
                    ignorePhase = 5;
                else if (ignorePhase > 0)
                    ignorePhase--;
                else if (ignorePhase == 0)
                {
                    if (delta.y <= -100)
                    {
                        SendMessage("OnShrink");
                        yield break;
                    }

                }
                yield return null;
            }
        }
    }
}
