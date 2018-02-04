using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UssValues : MonoBehaviour
{
    public static Dictionary<string, UssValue> values { get; private set; }

    static UssValues()
    {
        Reset();    
    }

    public static void Reset()
    {
        values = new Dictionary<string, UssValue>();
    }

    public static void SetValue(string key, UssValue value)
    {
        values[key] = value;
    }
    public static void SetValue(string key, string value)
    {
        values[key] = new UssStringValue() { body = value, value = value };
    }
    public static void SetValue(string key, Color value)
    {
        values[key] = new UssColorValue() { body = value.ToString(), value = value };
    }
    public static void SetValue(string key, int value)
    {
        values[key] = new UssIntValue() { body = value.ToString(), value = value };
    }
    public static void SetValue(string key, float value)
    {
        values[key] = new UssFloatValue() { body = value.ToString(), value = value };
    }

    public static UssValue GetValue(UssRefValue refValue)
    {
        return GetValue(refValue.key);
    }
    public static UssValue GetValue(string key)
    {
        if (values.ContainsKey(key) == false)
            throw new ArgumentException("Value `" + key + "` does not exists.");
        return values[key];
    }
}
