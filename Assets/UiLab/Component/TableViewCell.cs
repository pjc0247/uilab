using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class TableViewCell : UiBase,
    IPointerEnterHandler, IPointerExitHandler
{
    public VerticalTableView tableView;

    private bool isEntered = false;
    private Vector3 downPosition;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isEntered = true;
        StartCoroutine(ScrollFunc());
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        isEntered = false;
        //StopAllCoroutines();
    }
    IEnumerator ScrollFunc()
    {
        while (Input.GetMouseButton(0) == false)
            yield return null;

        downPosition = Input.mousePosition;
        var originalTableViewY = tableView.positionY;
        while (Input.GetMouseButton(0))
        {
            var delta = Input.mousePosition - downPosition;

            if (delta.y < -10)
                rt.pivot = new Vector2(rt.pivot.x, 1);
            else if (delta.y > 10)
                rt.pivot = new Vector2(rt.pivot.x, 0);

            if (delta.y > 300)
            {
                if (tableView.ScrollDown())
                {
                    yield return new WaitForSeconds(1.0f / 60 * 11);
                    ScaleTo(5, Vector3.one, Easing.SineOut);
                    yield break;
                }
                else break;
            }
            else if (delta.y < -300)
            {
                if (tableView.ScrollUp())
                {
                    yield return new WaitForSeconds(1.0f / 60 * 11);
                    ScaleTo(5, Vector3.one, Easing.SineOut);
                    yield break;
                }
                else break;
            }

            transform.localScale = new Vector3(1, 1 + Mathf.Clamp(Mathf.Abs(delta.y / 1000), 0, 0.3f), 1);

            yield return null;
        }

        if (rt.pivot.y == 1)
            ScaleTo(25, Vector3.one, Easing.ElasticInOut);
        else
            ScaleTo(5, Vector3.one, Easing.SineOut);
    }
}
