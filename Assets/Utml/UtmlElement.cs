using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtmlElementName : Attribute
{
    public string name;

    public UtmlElementName(string name)
    {
        this.name = name;
    }
}

public class UtmlElement : MonoBehaviour
{
}
