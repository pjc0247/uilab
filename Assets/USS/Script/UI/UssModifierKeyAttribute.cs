using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Method)]
public class UssModifierKeyAttribute : Attribute
{
    public string key;

    public UssModifierKeyAttribute(string _key)
    {
        key = _key;
    }
}
