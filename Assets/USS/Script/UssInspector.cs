using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class UssInspector : MonoBehaviour
{
    public DateTime updatedAt;
    public List<UssStyleDefinition> applied = new List<UssStyleDefinition>();

    public void Clear()
    {
        applied.Clear();
    }
}
