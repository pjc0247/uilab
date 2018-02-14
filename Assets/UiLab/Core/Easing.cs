using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Easing
{
    public delegate float EasingDelegate(float t);

    private static float Clamp(float t)
    {
        return Mathf.Clamp(t, 0, 1);
    }

    public static float Linear(float t)
    {
        return Flat(t);
    }
    public static float Flat(float t)
    {
        t = Clamp(t);
        return t;
    }

    public static float QuadIn(float t)
    {
        t = Clamp(t);
        return t * t;
    }
    public static float QuadOut(float t)
    {
        t = Clamp(t);
        return -1 * t * (t - 2);
    }
    public static float QuadInOut(float t)
    {
        t = Clamp(t);
        t = t * 2;
        if (t < 1)
            return 0.5f * t * t;
        --t;
        return -0.5f * (t * (t - 2) - 1);
    }

    public static float SineIn(float t)
    {
        t = Clamp(t);
        return -1 * Mathf.Cos(t * 3.14f / 2) + 1;
    }
    public static float SineOut(float t)
    {
        t = Clamp(t);
        return Mathf.Sin(t * 3.14f / 2);
    }

    public static float ExpoIn(float t)
    {
        t = Clamp(t);
        return t == 0 ? 0 : Mathf.Pow(2, 10 * (t / 1 - 1)) - 1 * 0.001f;
    }
    public static float ExpoOut(float t)
    {
        t = Clamp(t);
        return t == 1 ? 1 : (-Mathf.Pow(2, -10 * t / 1) + 1);
    }
    public static float ExpoInOut(float t)
    {
        t = Clamp(t);
        if (t == 0 || t == 1)
            return t;

        if (t < 0.5f)
            return 0.5f * Mathf.Pow(2, 10 * (t * 2 - 1));

        return 0.5f * (-Mathf.Pow(2, -10 * (t * 2 - 1)) + 2);
    }

    public static float BounceIn(float t)
    {
        t = Clamp(t);
        return 1 - BounceOut(1 - t);
    }
    public static float BounceOut(float t)
    {
        t = Clamp(t);
        if (t < (1.0f / 2.75f))
            return 7.5625f * t * t;
        if (t < (2.0f / 2.75f))
        {
            t -= 1.5f / 2.75f;
            return 7.5625f * t * t + 0.75f;
        }
        if (t < (2.5f / 2.75f))
        {
            t -= 2.25f / 2.75f;
            return 7.5625f * t * t + 0.9375f;
        }
        t -= 2.625f / 2.75f;
        return 7.5625f * t * t + 0.984375f;
    }
    public static float BounceInOut(float t)
    {
        t = Clamp(t);
        if (t < 0.5f)
            return BounceIn(t * 2) * 0.5f;
        else
            return BounceOut(t * 2 - 1) * 0.5f + 0.5f;
    }

    public static float BackIn(float t)
    {
        t = Clamp(t);
        float overshoot = 1.70158f;
        return t * t * ((overshoot + 1) * t - overshoot);
    }
    public static float BackOut(float t)
    {
        t = Clamp(t);
        float overshoot = 1.70158f;

        t = t - 1;
        return t * t * ((overshoot + 1) * t + overshoot) + 1;
    }
    public static float BackInOut(float t)
    {
        t = Clamp(t);
        float overshoot = 1.70158f * 1.525f;

        t = t * 2;
        if (t < 1)
        {
            return (t * t * ((overshoot + 1) * t - overshoot)) / 2;
        }
        else
        {
            t = t - 2;
            return (t * t * ((overshoot + 1) * t + overshoot)) / 2 + 1;
        }
    }

    public static float ElasticIn(float t)
    {
        t = Clamp(t);
        if (t == 0) return 0;
        if (t == 1) return 1;

        float s = 0.3f / 4;
        t = t - 1;
        return -Mathf.Pow(2, 10 * t) * Mathf.Sin((t - s) * (3.14f * 2) / 0.3f);
    }
    public static float ElasticOut(float t)
    {
        t = Clamp(t);
        if (t == 0) return 0;
        if (t == 1) return 1;

        float s = 0.3f / 4;

        return Mathf.Pow(2, -10 * t) * Mathf.Sin((t - s) * (3.14f * 2) / 0.3f) + 1;
    }
    public static float ElasticInOut(float t)
    {
        t = Clamp(t);
        if (t == 0) return 0;
        if (t == 1) return 1;

        t = t * 2;

        float s = 0.3f / 4;
        t = t - 1;
        if (t < 0)
            return -0.5f * Mathf.Pow(2, 10 * t) * Mathf.Sin((t - s) * (3.14f * 2) / 0.3f);
        else
            return Mathf.Pow(2, -10 * t) * Mathf.Sin((t - s) * (3.14f * 2) / 0.3f) * 0.5f + 1;
    }
}
