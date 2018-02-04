using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UssModifierException : Exception
{
    public UssModifierException(string className, UssValue value, Type expected) :
        base(className + " : " + value.GetType().Name + ". (expected " + expected.Name + ")")
    {
    }
}
