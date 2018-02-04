using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UssTokenType
{
    None,
    Whitespace,
    CrLf,

    Comment,

    LeftBracket, RightBracket,
    Colon, SemiColon,
    Comma,
    RightArrow,

    Asterisk,

    Id,
    ValueRef,
    Null,

    Int, Float, HexColor
}