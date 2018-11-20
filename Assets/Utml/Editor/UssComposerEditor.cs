using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UssComposerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var c = ((UtmlRoot)target);
    }
}