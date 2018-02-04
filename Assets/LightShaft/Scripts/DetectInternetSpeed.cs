using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectInternetSpeed : MonoBehaviour {

    private void Start()
    {
        StartCoroutine(CallWebPage());
    }
    IEnumerator CallWebPage()
    {
        DateTime dt1 = DateTime.Now;
        WWW www = new WWW("https://static.pexels.com/photos/20974/pexels-photo.jpg");
        yield return www;
        DateTime dt2 = DateTime.Now;
        Debug.Log(Math.Round((www.bytes.Length / 1024) / (dt2 - dt1).TotalSeconds, 2));
    }
}
