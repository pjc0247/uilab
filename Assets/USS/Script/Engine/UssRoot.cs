using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class UssRoot : UssIndicator
{
    [HideInInspector]
    public string ucssPath;
    public Object ucss;

    public static UssRoot FindRootInScene()
    {
        Component root = FindObjectOfType<UssRoot>();

        if (root == null)
        {
            Debug.LogWarning("There's no `UssRoot` object in this scene.");

            root = FindObjectOfType<Canvas>();
            if (root != null)
            {
                root.gameObject.AddComponent<UssRoot>();
                Debug.LogWarning("Use " + root.name + " instead.");
            }
        }

        return root.GetComponent<UssRoot>();
    }
}
