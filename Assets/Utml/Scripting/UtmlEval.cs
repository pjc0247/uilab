using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtmlEval
{
    
    // {{aa}}
    public static string EvalTemplateString(object binding, string tstr)
    {
        var regx = new Regex("^{{\\s*(.*)\\s*}}$");
        var name = regx.Match(tstr).Groups[0].Value;

        return 
            binding.GetType().GetProperty(name).GetValue(binding, new object[] { })
            .ToString();
    }
}
