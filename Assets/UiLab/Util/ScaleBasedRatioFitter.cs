using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ScaleBasedRatioFitter : MonoBehaviour
{
    public Image image;

    private Vector3 originalScale;

    void Awake()
    {
        if (image == null)
            image = GetComponent<Image>();
        originalScale = transform.localScale;
    }
	void Start ()
    {
		
	}

    void LateUpdate()
    {
        var ratio = image.rectTransform.sizeDelta.x / image.rectTransform.sizeDelta.y;
        var scaleRatio = transform.lossyScale.x / transform.lossyScale.y;

        transform.localScale = new Vector3(
            (transform.localScale.y * originalScale.y / transform.lossyScale.y) * (ratio / scaleRatio),
            transform.localScale.y,
            transform.localScale.z);
    }
}
