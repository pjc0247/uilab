using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UtmlAutoRefresh : AssetPostprocessor
{
    public static string currentUcss
    {
        get { return UssRoot.FindRootInScene().ucssPath; }
    }

    public static void EnsureLastUcssLoaded()
    {
        if (UssStyleModifier.loaded) return;

        if (string.IsNullOrEmpty(currentUcss) == false)
            UssStyleModifier.LoadUss(File.ReadAllText(currentUcss));
    }

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (var asset in importedAssets)
        {
            if (asset.EndsWith(".utml") == false)
                continue;

            // TODO: Optimise
            foreach (var obj in GameObject.FindObjectsOfType<UtmlRoot>())
            {
                Debug.Log(obj.utmlPath + " / " + asset);
                if (obj.utmlPath == asset)
                    obj.Rebuild(File.ReadAllText(asset));
            }
            //if (asset == currentUcss)
            //    UssComposer.load(File.ReadAllText(asset));
        }
    }
}
