using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UssUnexpectedTokenException : UssParsingException
{
    public UssToken token;
    public UssTokenType expected;

    public UssUnexpectedTokenException(UssToken _token) :
        base("Unexpected token: " + _token.body + ".")
    {
        source = ExceptionSource.Parser;

        token = _token;
    }
    public UssUnexpectedTokenException(UssToken _token, UssTokenType _expected) :
        base("Unexpected token: " + _token.body + ". (expected " + _expected + ")")
    {
        source = ExceptionSource.Parser;

        token = _token;
        expected = _expected;
    }
}
