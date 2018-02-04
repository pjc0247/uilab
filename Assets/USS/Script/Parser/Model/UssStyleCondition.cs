using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UssStyleConditionType
{
    None,
    DirectDescendant
}
public class UssStyleCondition
{
    public UssSelectorType target;
    public UssStyleConditionType type;
    public string name;

    public string targetState;
}