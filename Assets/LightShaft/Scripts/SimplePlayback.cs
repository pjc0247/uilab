using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using YoutubeLight;
using SimpleJSON;

public class SimplePlayback : Content {
	public string videoId = "bc0sJvtKrRM";
	private string videoUrl;
	private bool videoAreReadyToPlay = false;
	//use unity player(all platforms) or old method to play in mobile only if you want, or if your mobile dont support the new player.
	public bool useNewUnityPlayer;
	public VideoPlayer unityVideoPlayer;
	//start playing the video
	public bool playOnStart = false;
    public YoutubeLogo youtubeLogo;
    RequestResolver resolver;

	public void Start(){
        resolver = gameObject.AddComponent<RequestResolver>();

		if (playOnStart) {
			PlayYoutubeVideo (videoId);
		}
	}

    public void OnExpandContent()
    {
        PlayYoutubeVideo(videoId);
    }
    public void OnShrinkContent()
    {
        unityVideoPlayer.Stop();
    }
	public void PlayYoutubeVideo(string _videoId)
	{
        if(youtubeLogo != null)
        {
            youtubeLogo.youtubeurl = "https://www.youtube.com/watch?v=" + _videoId;
        }
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
                    videoAreReadyToPlay = false;
                    Debug.Log("Decript");
                    StartCoroutine(resolver.DecryptDownloadUrl(DecryptionFinished, info));
                    break;
                }
                else
                {
                    videoUrl = info.DownloadUrl;
                    videoAreReadyToPlay = true;
                }
                break;
            }
        }
    }

    public void DecryptionFinished(string url)
    {
        videoUrl = url;
        videoAreReadyToPlay = true;
    }

	bool checkIfVideoArePrepared = false;
	void FixedUpdate(){
		//used this to play in main thread.
		if (videoAreReadyToPlay) {
			videoAreReadyToPlay = false;
            //play using the old method
            if (!useNewUnityPlayer)
                StartHandheldVideo();
            else
            {
                Debug.Log("Play!!" + videoUrl);
                unityVideoPlayer.source = VideoSource.Url;
                unityVideoPlayer.url = videoUrl;
                checkIfVideoArePrepared = true;
                unityVideoPlayer.Prepare();
            }
		}

		if (checkIfVideoArePrepared) {
			checkIfVideoArePrepared = false;
			StartCoroutine (PreparingAudio ());
		}
	}


	IEnumerator PreparingAudio(){
		//Wait until video is prepared
		WaitForSeconds waitTime = new WaitForSeconds(1);
		while (!unityVideoPlayer.isPrepared)
		{
			Debug.Log("Preparing Video");
			//Prepare/Wait for 5 sceonds only
			yield return waitTime;
			//Break out of the while loop after 5 seconds wait
			break;
		}

		Debug.Log("Done Preparing Video");

		//Play Video
		unityVideoPlayer.Play();

		//Play Sound
		unityVideoPlayer.Play();

		Debug.Log("Playing Video");
		while (unityVideoPlayer.isPlaying)
		{
			yield return null;
		}
		OnVideoFinished ();
	}

	public void Play(){
		unityVideoPlayer.Play();
	}



	void StartHandheldVideo(){
#if UNITY_IPHONE || UNITY_ANDROID
        HandheldPlayback deviceplayer = gameObject.AddComponent<HandheldPlayback>();
        deviceplayer.PlayVideo(videoUrl, OnVideoFinished);
#endif
	}

	public void OnVideoFinished(){
		Debug.Log ("Video finished");
	}


}
