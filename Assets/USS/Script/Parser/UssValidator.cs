using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UssValidator
{ 
    public static bool IsValidInt(string str)
    {
        if (int.MaxValue.ToString().Length < str.Length)
            return false;
        return true;
    }
    public static bool IsValidFloat(string str)
    {
        if (16 < str.Length)
            return false;
        return true;
    }

    public static bool IsValidId(string id)
    {
        for (int i=0;i<id.Length;i++)
        {
            var c = id[i];

            if (i == 0 && (c == '.' || c == '#'))
                ; // PASS
            else if (c == '_' || c == '-' ||
                char.IsLetterOrDigit(c))
                ; // PASS
            else
                return false;
        }

        return true;
    }
    public static bool IsValidHexColor(string str)
    {
        if (str[0] != '#')
            return false;

        // [TODO] A~F check
        for (int i = 1; i < str.Length; i++)
        {
            var c = str[i];

            if (char.IsLetterOrDigit(c))
                ; // PASS
            else
                return false;
        }

        return true;
    }
    public static bool IsValidValueRef(string str)
    {
        if (str[0] != '@')
            return false;

        for (int i = 1; i < str.Length; i++)
        {
            var c = str[i];

            if (c == '_' ||
                char.IsLetterOrDigit(c))
                ; // PASS
            else
                return false;
        }

        return true;
    }
}
