using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UssInvalidTokenException : UssParsingException
{
    public UssInvalidTokenException(string token)
        : base("Invalid token: " + token)
    {
    }
}
