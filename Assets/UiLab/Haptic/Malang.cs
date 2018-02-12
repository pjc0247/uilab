using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class Malang : MonoBehaviour
{
    public float speedFactor = 2;

    private Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void DoHaptic()
    {
        StopAllCoroutines();
        StartCoroutine(StartFunc());
    }

    private Vector3 GetDiff(Vector3 target)
    {
        return new Vector3(originalScale.x * target.x, originalScale.y * target.y, transform.localScale.z) - transform.localScale;
    }
    IEnumerator StartFunc()
    {
        var x = speedFactor;
        // 1.1 , 0.95
        var step = new Vector3(transform.localScale.x * 0.2f, transform.localScale.y * -0.1f) / (4 * x);
        for (int i = 0; i < 4 * x; i++)
        {
            transform.localScale += step;
            yield return null;
        }

        // 0.9 , 1.1
        step = GetDiff(new Vector3(0.8f, 1.2f)) / (5 * x);
        for (int i = 0; i < 5 * x; i++)
        {
            transform.localScale += step;
            yield return null;
        }

        // 1.05 , 0.95
        step = GetDiff(new Vector3(1.1f, 0.9f)) / (4 * x);
        for (int i = 0; i < 4 * x; i++)
        {
            transform.localScale += step;
            yield return null;
        }

        // 1.0 , 1.0
        step = GetDiff(Vector3.one) / (6 * x);
        for (int i = 0; i < 6 * x; i++)
        {
            transform.localScale += step;
            yield return null;
        }

        transform.localScale = originalScale;
    }
}