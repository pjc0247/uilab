using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UssToken
{
    public UssTokenType type;

    public string body;

    public string file;
    public int line;
    public int col;

    public bool IsIgnoreable
    {
        get
        {
            return type == UssTokenType.Whitespace ||
                type == UssTokenType.CrLf; // ??
        }
    }
    public bool IsValue
    {
        get
        {
            return type == UssTokenType.Null ||
                type == UssTokenType.ValueRef ||
                type == UssTokenType.Int || type == UssTokenType.Float ||
                type == UssTokenType.HexColor ||
                type == UssTokenType.Id;
        }
    }
    public bool IsSyntaxStateChanger
    {
        get
        {
            return type == UssTokenType.LeftBracket || type == UssTokenType.RightBracket ||
                type == UssTokenType.SemiColon;
        }
    }
}