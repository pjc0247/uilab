using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UssParsingException : Exception
{
    public enum ExceptionSource
    {
        Lexer, Parser
    }

    public ExceptionSource source;

    public UssParsingException(string message) :
        base(message)
    {
    }
}
