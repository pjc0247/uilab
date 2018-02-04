using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using YoutubeLight;
using SimpleJSON;
using System.Text;
using System;

public class WebGlPlayback : MonoBehaviour
{
    /*PRIVATE INFO DO NOT CHANGE THESE URLS OR VALUES*/
    private const string serverURI = "https://unity-dev-youtube.herokuapp.com/api/info?url=https://www.youtube.com/watch?v=";
    private const string formatURI = "&format=best&flatten=true";
    private const string videoURI = "https://youtubewebgl.herokuapp.com/download.php?mime=video/mp4&title=generatedvideo&token=";
    /*END OF PRIVATE INFO*/

    [SerializeField]
    public YoutubeResultIds webGlResults;

    public string videoId = "bc0sJvtKrRM";
    private string videoUrl;
    private string audioVideoUrl;
    //If you will use high quality playback we need one video player that only will run the audio.
    public VideoPlayer unityVideoPlayer;
    //start playing the video
    public bool playOnStart = false;

    private bool noHD = false;

    RequestResolver resolver;

    public void Start()
    {
        resolver = gameObject.AddComponent<RequestResolver>();
        if (playOnStart)
        {
            PlayYoutubeVideo(videoId);
        }
    }


    public void PlayYoutubeVideo(string _videoId)
    {
#if UNITY_WEBGL
        if (this.GetComponent<VideoController>() != null)
        {
                this.GetComponent<VideoController>().ShowLoading("Loading...");
        }
        videoId = _videoId;
        StartCoroutine(WebGlRequest(videoId));
#else
        Debug.LogError("Please use this script only for webgl");
#endif

    }

    IEnumerator WebGlRequest(string videoID)
    {
        WWW request = new WWW(serverURI+""+videoID+""+formatURI);
        yield return request;
        var requestData = JSON.Parse(request.text);
        var videos = requestData["videos"][0]["formats"];
        webGlResults.bestFormatWithAudioIncluded = requestData["videos"][0]["url"];

        for (int counter = 0; counter < videos.Count; counter++) {
            if(videos[counter]["format_id"] == "160")
            {
                webGlResults.lowQuality = videos[counter]["url"];
            }else if (videos[counter]["format_id"] == "133")
            {
                webGlResults.lowQuality = videos[counter]["url"];   //if have 240p quality overwrite the 144 quality as low quality.
            }else if(videos[counter]["format_id"] == "134")
            {
                webGlResults.standardQuality = videos[counter]["url"];  //360p
            }else if(videos[counter]["format_id"] == "135"){
                webGlResults.mediumQuality = videos[counter]["url"];  //480p
            }else if (videos[counter]["format_id"] == "136")
            {
                webGlResults.hdQuality = videos[counter]["url"];  //720p
            }else if(videos[counter]["format_id"] == "137")
            {
                webGlResults.fullHdQuality = videos[counter]["url"];  //1080p
            }else if (videos[counter]["format_id"] == "266")
            {
                webGlResults.ultraHdQuality = videos[counter]["url"];  //@2160p 4k
            }else if (videos[counter]["format_id"] == "139")
            {
                webGlResults.audioUrl = videos[counter]["url"];  //AUDIO
            }
        }
        //quality selection will be implemented later for webgl, i recomend use the  webGlResults.bestFormatWithAudioIncluded
        WebGlGetVideo(webGlResults.bestFormatWithAudioIncluded);
    }



    //WEBGL only...
    public void WebGlGetVideo(string url)
    {
        byte[] bytesToEncode = Encoding.UTF8.GetBytes(url);
        string encodedText = Convert.ToBase64String(bytesToEncode);
        videoUrl = videoURI+""+ encodedText;
        
        //audioVideoUrl = videoUrl;

        Debug.Log("Play!! " + videoUrl);
        unityVideoPlayer.source = VideoSource.Url;
        unityVideoPlayer.url = videoUrl;
        
        unityVideoPlayer.Prepare();
        //audioVplayer.source = VideoSource.Url;
        //audioVplayer.url = audioVideoUrl;
        //audioVplayer.Prepare();

        videoPrepared = false;
        unityVideoPlayer.prepareCompleted += VideoPrepared;
       
        //audioPrepared = false;
        //audioVplayer.prepareCompleted += AudioPrepared;
       
    }

    IEnumerator WebGLPlay() //The prepare not respond so, i forced to play after some seconds
    {
        yield return new WaitForSeconds(2f);
        Play();
    }


    private bool audioDecryptDone = false;
    private bool videoDecryptDone = false;

    public VideoPlayer audioVplayer;

    bool startedPlaying = false;

    void FixedUpdate()
    {
        if(unityVideoPlayer.isPrepared)
        {
            if (!startedPlaying)
            {
                startedPlaying = true;
                StartCoroutine(WebGLPlay());
            }
        }
        
        CheckIfIsDesync();
    }

    private bool videoPrepared;
    private bool audioPrepared;

    void AudioPrepared(VideoPlayer vPlayer)
    {
        audioVplayer.prepareCompleted -= AudioPrepared;
        audioPrepared = true;
        if (audioPrepared && videoPrepared)
            Play();
    }

    void VideoPrepared(VideoPlayer vPlayer)
    {
        unityVideoPlayer.prepareCompleted -= VideoPrepared;
        videoPrepared = true;
        Debug.Log("Playing youtube video only, the audio separated will be implemented in the final release of webgl support");
        noHD = true; //force now to prevent bugs...
        Play(); 
    }

    public void Play()
    {
        unityVideoPlayer.loopPointReached += PlaybackDone;
        StartCoroutine(WaitAndPlay());
    }

    private void PlaybackDone(VideoPlayer vPlayer)
    {
        OnVideoFinished();
    }

    IEnumerator WaitAndPlay()
    {

        if (!noHD)
        {
            audioVplayer.Play();
            if (syncIssue)
                yield return new WaitForSeconds(0.35f);
            else
                yield return new WaitForSeconds(0);
        }
        else
        {
            if (syncIssue)
                yield return new WaitForSeconds(1f);//if is no hd wait some more
            else
                yield return new WaitForSeconds(0);
        }
        unityVideoPlayer.Play();
        if (this.GetComponent<VideoController>() != null)
        {
            this.GetComponent<VideoController>().HideLoading();
        }
    }

    IEnumerator StartVideo()
    {
#if UNITY_IPHONE || UNITY_ANDROID
		Handheld.PlayFullScreenMovie (videoUrl);
#endif
        yield return new WaitForSeconds(1.4f);
        OnVideoFinished();
    }

    public void OnVideoFinished()
    {
        if (unityVideoPlayer.isPrepared)
        {
            Debug.Log("Finished");
            if (unityVideoPlayer.isLooping)
            {
                unityVideoPlayer.time = 0;
                unityVideoPlayer.frame = 0;
                audioVplayer.time = 0;
                audioVplayer.frame = 0;
                unityVideoPlayer.Play();
                audioVplayer.Play();
            }
        }
    }

    public enum VideoQuality
    {
        mediumQuality,
        Hd720,
        Hd1080,
        Hd1440,
        Hd2160
    }

    [HideInInspector]
    public bool isSyncing = false;

    [Header("If you think audio is out of sync enable this bool below")]
    [Header("This happens in some unity versions, the most stable is the 5.6.1p1")]
    public bool syncIssue;

    //Experimental
    private void CheckIfIsDesync()
    {
        if (!noHD)
        {
            //Debug.Log(unityVideoPlayer.time+" "+ audioVplayer.time);
            double t = unityVideoPlayer.time - audioVplayer.time;
            if (!isSyncing)
            {
                if (t != 0)
                {
                    Sync();
                }
                else if (unityVideoPlayer.frame != audioVplayer.frame)
                {
                    Sync();
                }
            }
        }
        else
        {
            //unityVideoPlayer.frame -= 1;
        }
    }

    private void Sync()
    {
        VideoController controller = GameObject.FindObjectOfType<VideoController>();
        if (controller != null)
        {
            //isSyncing = true;
            //audioVplayer.time = unityVideoPlayer.time;
            //audioVplayer.frame = unityVideoPlayer.frame;
            //controller.Seek();
        }
        else
        {
            Debug.LogWarning("Please add a video controller to your scene to make the sync work! Will be improved in the future.");
        }
    }

    public int GetMaxQualitySupportedByDevice()
    {
        if (Screen.orientation == ScreenOrientation.Landscape)
        {
            //use the height
            return Screen.currentResolution.height;
        }
        else if (Screen.orientation == ScreenOrientation.Portrait)
        {
            //use the width
            return Screen.currentResolution.width;
        }
        else
        {
            return Screen.currentResolution.height;
        }
    }
}

[System.Serializable]
public class YoutubeResultIds
{
    public string lowQuality;
    public string standardQuality;
    public string mediumQuality;
    public string hdQuality;
    public string fullHdQuality;
    public string ultraHdQuality;
    public string bestFormatWithAudioIncluded;
    public string audioUrl;
    
}
