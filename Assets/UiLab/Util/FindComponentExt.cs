using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FindComponentExt
{
    public static T AssignIfNull<T>(this T c, MonoBehaviour _this, string name)
        where T : Component
    {
        if (c != null) return c;
        return _this.transform.Find(name).GetComponent<T>();
    }
}
