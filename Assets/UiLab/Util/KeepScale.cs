using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepScale : MonoBehaviour
{
    private Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.lossyScale;
    }
	void Update ()
    {
        transform.localScale =
            new Vector3(
                transform.localScale.x * originalScale.x / transform.lossyScale.x,
                transform.localScale.y * originalScale.y / transform.lossyScale.y,
                transform.localScale.z * originalScale.z / transform.lossyScale.z
                );
	}
}
