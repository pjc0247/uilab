using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThreeStepDiagonal : MonoBehaviour
{
    public Color color = Color.red;
    [Range(0, 2)] public float strength = 1;

    public Image a, b, c;

    void OnValidate()
    {
        a.color = new Color(color.r, color.g, color.b, (100 * strength) / 255);
        b.color = new Color(color.r, color.g, color.b, (158 * strength) / 255);
        c.color = new Color(color.r, color.g, color.b, (255 * strength) / 255);
    }
}
