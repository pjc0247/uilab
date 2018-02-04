using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Highlighter : MonoBehaviour
{
    public Text errorMessage;

    private List<Graphic> prevObjects = new List<Graphic>();

	void Start () {
		
	}

    public void OnSelectorChanged(string text)
    {
        var conditions = UssParser.ParseConditions(text);
        var go = UssStyleModifier.FindObjects(
            UssRoot.FindRootInScene().gameObject, conditions);

        foreach (var c in prevObjects)
        {
            if (go.Contains(c.gameObject)) continue;
            c.CrossFadeColor(Color.white, 0, true, true);
        }
        prevObjects.Clear();

        foreach (var g in go)
        {
            foreach (var c in g.GetComponents<Graphic>())
            {
                c.CrossFadeColor(Color.red, 0.5f, true, true);
                prevObjects.Add(c);
            }
        }
    }

    public void OnStyleSheetChanged(string text)
    {
        try
        {
            var styles = UssParser.Parse(text);
            UssStyleModifier.Apply(
                UssRoot.FindRootInScene().gameObject, styles.styles);

            errorMessage.CrossFadeColor(Color.black, 0.5f, true, true);
            errorMessage.text = "SUCCESS";
        }
        catch (Exception e)
        {
            errorMessage.CrossFadeColor(Color.red, 0.5f, true, true);
            errorMessage.text = e.Message;
        }
    }
}
