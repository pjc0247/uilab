using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YoutubeLight;
using System;

public class HandheldPlayback : MonoBehaviour {

    RequestResolver resolver;
    private string videoUrl;

    public static HandheldPlayback instance;

    Action videoFinishCallback;

    private void Start()
    {
        instance = this;
        resolver = gameObject.AddComponent<RequestResolver>();
    }

    public void PlayVideo(string url, Action OnVideoFinished)
    {
        videoFinishCallback = OnVideoFinished;
        StartCoroutine(resolver.GetDownloadUrls(FinishLoadingUrls, url, false));
    }

    void FinishLoadingUrls()
    {
        List<VideoInfo> videoInfos = resolver.videoInfos;
        foreach (VideoInfo info in videoInfos)
        {
            if (info.VideoType == VideoType.Mp4 && info.Resolution == (720))
            {
                if (info.RequiresDecryption)
                {
                    //The string is the video url
                    StartCoroutine(resolver.DecryptDownloadUrl(DecryptionFinished, info));
                    break;
                }
                else
                {
                    StartCoroutine(Play(info.DownloadUrl));
                }
                break;
            }
        }
    }

    public void DecryptionFinished(string url)
    {
        StartCoroutine(Play(url));
    }

    IEnumerator Play(string url)
    {
        Debug.Log("Play!");
#if UNITY_IPHONE || UNITY_ANDROID
        Handheld.PlayFullScreenMovie(url, Color.black, FullScreenMovieControlMode.Full, FullScreenMovieScalingMode.Fill);
#else
        Debug.Log("This only runs in mobile");
#endif
        yield return new WaitForSeconds(1);
        videoFinishCallback.Invoke();
    }
}
