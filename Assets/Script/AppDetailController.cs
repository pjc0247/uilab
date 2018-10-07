using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppDetailController : MonoBehaviour
{
    public static string pageName;

	void Start () {
		
	}

    public void OnClickYoutube(string url)
    {
        Application.OpenURL(url);
    }

    void OnEnable()
    {
        var trs = transform.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in trs)
        {
            if (transform == t) continue;
            if (t.parent != transform) continue;

            if (t.name == pageName)
                t.gameObject.SetActive(true);
            else
                t.gameObject.SetActive(false);
        }
    }
}
