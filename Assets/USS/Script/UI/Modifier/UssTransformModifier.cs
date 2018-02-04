using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UssTransformModifier : MonoBehaviour
{
    [UssModifierKey("rotation")]
    public void ApplyRotation(GameObject g, UssValue[] values)
    {
        var original = g.transform.localEulerAngles;

        if (values.Length == 1)
        {
            g.transform.localEulerAngles = new Vector3(original.x, original.y, values[0].AsFloat());
        }
        else if (values.Length == 3)
        {
            g.transform.localEulerAngles = new Vector3(values[0].AsFloat(), values[1].AsFloat(), values[2].AsFloat());
        }
    }
}
