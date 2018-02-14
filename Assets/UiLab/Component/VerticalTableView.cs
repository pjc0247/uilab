using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[DisallowMultipleComponent]
public class VerticalTableView : UiBase
{
    public int firstItemIndex { get; private set; }

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
                childRt.anchoredPosition = new Vector2(childRt.anchoredPosition.x, offset - cellHeight);

            var cell = c.GetComponent<TableViewCell>();
            if (cell == null) cell = c.gameObject.AddComponent<TableViewCell>();
            cell.tableView = this;

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
}
