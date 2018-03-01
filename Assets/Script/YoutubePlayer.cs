using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class YoutubePlayer : UiBase
{
    public Image spinner, play;

    private RawImage rawImage;
    private WebGlPlayback youtube;

    private RenderTexture videoRT;

    private bool requestedPlay = false;

    protected override void Awake()
    {
        base.Awake();

        spinner.CrossFadeAlpha(0, 0, true);
    }
    void Start()
    {
        rawImage = GetComponent<RawImage>();
        youtube = GetComponent<WebGlPlayback>();

        YoutubeAPIManager.instance.GetVideoData(youtube.videoId, OnReceiveVideoData);
    }
    private void OnReceiveVideoData(YoutubeData videoData)
    {
        StartCoroutine(DownloadThumbnailFunc(videoData));
    }
    IEnumerator DownloadThumbnailFunc(YoutubeData videoData)
    {
        var url = videoData.snippet.thumbnails.mediumThumbnail.url;
        var www = UnityWebRequestTexture.GetTexture(url);

        yield return www.SendWebRequest();

        var tex = DownloadHandlerTexture.GetContent(www);
        rawImage.texture = tex;

        videoRT = new RenderTexture(tex.width, tex.height, 24);
        youtube.unityVideoPlayer.targetTexture = videoRT;
        GetComponent<AspectRatioFitter>().aspectRatio = (float)tex.width / tex.height;
    }

    public void Play()
    {
        if (requestedPlay) return;

        requestedPlay = true;
        youtube.PlayYoutubeVideo(youtube.videoId);
        StartCoroutine(WaitForPlay());
    }
    IEnumerator WaitForPlay()
    {
        play.CrossFadeAlpha(0, 0.2f, true);
        spinner.CrossFadeAlpha(1, 0.2f, true);
        while (youtube.unityVideoPlayer.isPlaying == false)
        {
            spinner.transform.Rotate(0, 0, 10);
            yield return null;
        }
        spinner.CrossFadeAlpha(0, 0.5f, true);

        rawImage.texture = videoRT;
    }
}

