using UnityEngine;
using System.Collections;
using YoutubeLight;
using System.Collections.Generic;

public class YoutubeEasyMovieTexture : MonoBehaviour
{
    public string videoId = "bc0sJvtKrRM";
    private string videoUrl;
    RequestResolver resolver;

    public void Play()
    {
        resolver = gameObject.AddComponent<RequestResolver>();
        GetURL(videoId);
    }

    public void GetURL(string _videoId)
    {
        videoId = _videoId;
        StartCoroutine(resolver.GetDownloadUrls(FinishLoadingUrls, videoId, false));
    }

    void FinishLoadingUrls()
    {
        List<VideoInfo> videoInfos = resolver.videoInfos;
        foreach (VideoInfo info in videoInfos)
        {
            if (info.VideoType == VideoType.Mp4 && info.Resolution == (360))
            {
                if (info.RequiresDecryption)
                {
                    //The string is the video url
                    Debug.Log("Decript");
                    StartCoroutine(resolver.DecryptDownloadUrl(DecryptionFinished, info));
                    break;
                }
                else
                {
                    videoUrl = info.DownloadUrl;
                }
                break;
            }
        }
    }

    public void DecryptionFinished(string url)
    {
        videoUrl = url;
        /*
         *  IF YOU HAVE EASY MOVIE TEXTURE, ADD THIS SCRIPT IN THE SAME GAME OBJECT AS THE "MediaPlayerCtrl" ARE
         *  Then uncomment these lines below.
         */
        //this.gameObject.GetComponent<MediaPlayerCtrl>().m_strFileName = videoUrl;
        //this.gameObject.GetComponent<MediaPlayerCtrl>().Play(); //---> or equivalent if the MediaPlayerCtrl is not setted to play auto at start.
        
    }
}
