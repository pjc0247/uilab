using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ToggleButton : UiBase,
    IPointerClickHandler
{
    public Image state1, state2;
    public float duration = 0.1f;
    public UnityEvent onClick;

    private bool isStateOne = true;

    void Start ()
    {
		
	}
    void OnValidate()
    {
        state1.SetAlpha(1);
        state2.SetAlpha(0);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick.Invoke();

        isStateOne ^= true;
        if (isStateOne)
        {
            state1.AlphaTo(1, duration);
            state2.AlphaTo(0, duration);
        }
        else
        {
            state1.AlphaTo(0, duration);
            state2.AlphaTo(1, duration);
        }

        SendMessage("DoHaptic");
    }
}
