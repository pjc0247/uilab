using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UssValue
{
    public string body;

    public static UssValue Create(UssToken token)
    {
        if (token.IsValue == false)
            throw new ArgumentException("token is not a value");

        if (token.type == UssTokenType.Null)
            return new UssNullValue() { body = token.body };
        if (token.type == UssTokenType.Int)
            return new UssIntValue() { body = token.body, value = int.Parse(token.body) };
        if (token.type == UssTokenType.Float)
            return new UssFloatValue() { body = token.body, value = float.Parse(token.body) };
        if (token.type == UssTokenType.Id)
            return new UssStringValue() { body = token.body, value = token.body };
        if (token.type == UssTokenType.HexColor) {
            Color color;
            if (ColorUtility.TryParseHtmlString(token.body, out color))
                return new UssColorValue() { body = token.body, value = color };
            else
                throw new UssParsingException("Invalid color format: " + token.body);
        }
        if (token.type == UssTokenType.ValueRef)
            return new UssRefValue() { body = token.body, key = token.body.Substring(1) };

        throw new InvalidOperationException("Unknown type: " + token.type);
    }

    public bool IsNone()
    {
        if (GetType() == typeof(UssStringValue))
            return ((UssStringValue)this).value == "none";
        return false;
    }
}
public class UssValueBase<T> : UssValue
{
    public T value;
}
public static class UssValueExt
{
    public static UssValue Unwrap(this UssValue v)
    {
        if (v.GetType() == typeof(UssRefValue))
            return UssValues.GetValue((UssRefValue)v);
        return v;
    }

    public static Color AsColor(this UssValue v)
    {
        v = v.Unwrap();

        if (v.GetType() == typeof(UssColorValue))
            return ((UssColorValue)v).value;
        if (v.GetType() == typeof(UssStringValue))
        {
            var str = v.AsString();
            if (str == "black")
                return Color.black;
            else if (str == "green")
                return Color.green;
            else if (str == "red")
                return Color.red;
            else if (str == "blue")
                return Color.blue;
        }

        throw new InvalidOperationException("Value cannot be color: " + v.GetType());
    }
    public static string AsString(this UssValue v)
    {
        v = v.Unwrap();

        if (v.GetType() == typeof(UssStringValue))
            return ((UssStringValue)(object)v).value;
        if (v.GetType() == typeof(UssNullValue))
            return null;

        throw new InvalidOperationException("Value cannot be string: " + v.GetType());
    }
    public static float AsFloat(this UssValue v)
    {
        v = v.Unwrap();

        if (v.GetType() == typeof(UssFloatValue))
            return ((UssFloatValue)(object)v).value;
        if (v.GetType() == typeof(UssIntValue))
            return ((UssIntValue)(object)v).value;

        throw new InvalidOperationException("Value cannot be float: " + v.GetType());
    }
    public static int AsInt(this UssValue v)
    {
        v = v.Unwrap();

        if (v.GetType() == typeof(UssFloatValue))
            return (int)((UssFloatValue)(object)v).value;
        if (v.GetType() == typeof(UssIntValue))
            return ((UssIntValue)(object)v).value;

        throw new InvalidOperationException("Value cannot be integer: " + v.GetType());
    }
}
public class UssRefValue : UssValueBase<string>
{
    public string key;
}
public class UssNullValue : UssValueBase<object>
{
}
public class UssStringValue : UssValueBase<string>
{
}
public class UssIntValue : UssValueBase<int>
{
}
public class UssFloatValue : UssValueBase<float>
{
}
public class UssColorValue : UssValueBase<Color32>
{
}
public class UssBundleValue : UssValueBase<UssStyleProperty[]>
{
}