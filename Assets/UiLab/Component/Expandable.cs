using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Expandable : UiBase,
    IPointerClickHandler
{
    private bool isExpanded = false;

    void Start ()
    {
		
	}

    public void OnPointerClick(PointerEventData eventData)
    {
        isExpanded ^= true;

        if (isExpanded)
            Expand();
        else
            Shrink();
    }

    void Expand()
    {
        ScaleTo(15, Vector3.one * 2, Easing.QuadOut);
    }
    void Shrink()
    {
        ScaleTo(15, Vector3.one, Easing.QuadOut);
    }
}
