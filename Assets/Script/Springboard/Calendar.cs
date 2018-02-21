using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Calendar : MonoBehaviour
{
    public Text dayOfWeek, date;

    void Awake()
    {
        var now = DateTime.Now;

        date.text = now.Day.ToString();
        dayOfWeek.text = now.DayOfWeek.ToString().Substring(0, 3).ToUpper();
    }
	void Start ()
    {
		
	}
}
