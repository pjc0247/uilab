using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Lockscreen : MonoBehaviour
{
    public Text clock;
    public Text date;

    void Update()
    {
        var now = DateTime.Now;
        clock.text = now.Hour + ":" + (now.Minute < 10 ? "0" + now.Minute : now.Minute.ToString());
        date.text = now.DayOfWeek + ", " + now.ToString("MMMM", CultureInfo.InvariantCulture) + " " + now.Day;
    }
}
