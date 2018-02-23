using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
[DisallowMultipleComponent]
public class VerticalTableView : UiBase,
    IPointerEnterHandler, IPointerExitHandler
{
    public int firstItemIndex;

    private bool isPointerEntered = false;
    private Coroutine scrollCoro;

	void Start ()
    {
		
	}
    void OnValidate()
    {
        RefreshLayout();
    }
        
    void LateUpdate()
    {
        RefreshLayout();
    }
    public void RefreshLayout()
    {
        var offset = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            var c = transform.GetChild(i);
            var childRt = c.GetComponent<RectTransform>();
            var cellHeight = childRt.rect.height;

            if (childRt.pivot.y == 1)
            {
                childRt.anchoredPosition = new Vector2(childRt.anchoredPosition.x, offset);
                cellHeight *= childRt.localScale.y;
            }
            else if (childRt.pivot.y == 0)
            {
                childRt.anchoredPosition = new Vector2(childRt.anchoredPosition.x, offset - cellHeight * childRt.localScale.y);
                cellHeight *= childRt.localScale.y;
            }

            var cell = c.GetComponent<TableViewCell>();
            if (cell == null) cell = c.gameObject.AddComponent<TableViewCell>();
            cell.tableView = this;
            cell.index = i;
            cell.offset = offset;

            offset -= (int)(cellHeight);
        }
    }

    public bool ScrollUp()
    {
        if (firstItemIndex == 0) return false;

        var firstItem = transform.GetChild(firstItemIndex).GetComponent<RectTransform>();
        MoveTo(24, position - new Vector2(0, firstItem.rect.height), Easing.BounceOut);
        firstItemIndex--;

        return true;
    }
    public bool ScrollDown()
    {
        if (firstItemIndex == transform.childCount - 1) return false;

        var lastItem = transform.GetChild(firstItemIndex).GetComponent<RectTransform>();
        MoveTo(24, position + new Vector2(0, lastItem.rect.height), Easing.BounceOut);
        firstItemIndex++;

        return true;
    }
    public void ScrollTo(int index)
    {
        MoveTo(15, new Vector2(positionX, -GetCell(index).offset), Easing.ExpoOut);
        firstItemIndex = index;
    }

    public TableViewCell GetCell(int index)
    {
        return transform.GetChild(index).GetComponent<TableViewCell>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerEntered = true;

        scrollCoro = StartCoroutine(ScrollFunc());
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerEntered = false;
    }
    IEnumerator ScrollFunc()
    {
        while (isPointerEntered)
        {
            while (isPointerEntered && Input.GetMouseButtonDown(0) == false)
                yield return null;

            TableViewCell cell = null;
            var delta = Vector2.zero;
            var downPosition = Input.mousePosition;
            var originalTableViewY = positionY;
            var results = new List<RaycastResult>();
            raycaster.Raycast(new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            }, results);

            var tr = results[0].gameObject.transform;
            while(tr.parent != null)
            {
                cell = tr.GetComponent<TableViewCell>();
                if (cell != null)
                    break;
                tr = tr.parent;
            }

            if (cell == null) continue;

            while (isPointerEntered && Input.GetMouseButton(0))
            {
                delta = Input.mousePosition - downPosition;

                if (delta.y < -10)
                    cell.rt.pivot = new Vector2(cell.rt.pivot.x, 1);
                else if (delta.y > 10)
                    cell.rt.pivot = new Vector2(cell.rt.pivot.x, 0);

                if (delta.y > 150)
                {
                    if (ScrollDown())
                    {
                        //yield return new WaitForSeconds(1.0f / 60 * 11);
                        cell.ScaleTo(5, Vector3.one, Easing.SineOut);
                        break;
                    }
                    else break;
                }
                else if (delta.y < -150)
                {
                    if (ScrollUp())
                    {
                        //yield return new WaitForSeconds(1.0f / 60 * 11);
                        cell.ScaleTo(5, Vector3.one, Easing.SineOut);
                        break;
                    }
                    else break;
                }

                if (delta.y > 0 && cell.index != 0)
                {
                    GetCell(cell.index - 1).transform.localScale = new Vector3(1, 1 - Mathf.Clamp(Mathf.Abs(delta.y / 1000), 0, 0.3f), 1);
                }
                cell.transform.localScale = new Vector3(1, 1 + Mathf.Clamp(Mathf.Abs(delta.y / 1000), 0, 0.3f), 1);

                yield return null;
            }

            if (Mathf.Abs(delta.magnitude) <= 10)
                cell.gameObject.SendMessage("OnClickCell", SendMessageOptions.DontRequireReceiver);
            else
            {
                if (cell.index != 0)
                {
                    GetCell(cell.index - 1).ScaleTo(5, Vector3.one, Easing.SineOut);
                }
                if (cell.rt.pivot.y == 1)
                    yield return cell.ScaleTo(25, Vector3.one, Easing.SineOut);
                else
                    yield return cell.ScaleTo(5, Vector3.one, Easing.SineOut);

                while (isPointerEntered && Input.GetMouseButton(0))
                    yield return null;
            }
        }
    }
}
