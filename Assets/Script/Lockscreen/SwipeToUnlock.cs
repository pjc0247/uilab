using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SwipeToUnlock : UiBase,
    IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private RectTransform parentRt;

    void Start()
    {
        parentRt = transform.parent.GetComponent<RectTransform>();
    }
    public void OnDrag(PointerEventData eventData)
    {
        var rootRt = GetComponentInParent<CanvasScaler>().GetComponent<RectTransform>();
        var y = 2637.0f / Screen.height * Input.mousePosition.y;

        parentRt.anchoredPosition = new Vector2(0, y);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (parentRt.anchoredPosition.y >= 1024)
            MoveTo(parentRt, 8, new Vector2(0, 3000), Easing.SineOut);
        else
            MoveTo(parentRt, 13, new Vector2(0, 0), Easing.SineOut);
    }
}
