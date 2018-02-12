using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Profiling;

public class ImageLoader : MonoBehaviour
{
    private Texture2D tex;

    IEnumerator Start()
    {
        Debug.Log("Start");

        yield return new WaitForSeconds(2);

        Debug.Log("BeginDownload");
        var www = UnityWebRequestTexture.GetTexture("https://static.pexels.com/photos/236047/pexels-photo-236047.jpeg");
        yield return www.SendWebRequest();

        Debug.Log("EndDownload");
        Profiler.BeginSample("Texture");

        //tex = new Texture2D(2, 2);
        var texture = DownloadHandlerTexture.GetContent(www);

        //www.LoadImageIntoTexture(tex);
        Profiler.EndSample();
    }
}
