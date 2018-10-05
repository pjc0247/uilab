using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppDetailController : MonoBehaviour
{
    public static string pageName;

	void Start () {
		
	}

    void OnEnable()
    {
        var trs = transform.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in trs)
        {
            Debug.Log(t.name);

            if (t.name == pageName)
                t.gameObject.SetActive(true);
        }
    }
}
