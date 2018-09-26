using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class UtmlAssetLoader : MonoBehaviour
{
    public static T Load<T>(string path)
        where T : UnityEngine.Object
    {
#if UNITY_EDITOR
        if (Application.isPlaying == false)
        {
            Debug.Log(Application.dataPath + "/Resources/" + path);
            return AssetDatabase.LoadAssetAtPath<T>(
                "Assets/Resources/" + path);
        }
        else
            return Resources.Load<T>(path);
#else
        return Resources.Load<T>(path);
#endif
    }
}
