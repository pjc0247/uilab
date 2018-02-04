using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class YoutubeDemoUsage : MonoBehaviour {
    public GameObject mainUI;

	public void DemoPlayback()
    {
        //search for the low quality if not find search for highquality
        if (GameObject.FindObjectOfType<SimplePlayback>() != null)
        {
            GameObject.FindObjectOfType<SimplePlayback>().PlayYoutubeVideo("bc0sJvtKrRM");
            GameObject.FindObjectOfType<SimplePlayback>().unityVideoPlayer.loopPointReached += OnVideoFinished;
        }
        else if (GameObject.FindObjectOfType<HighQualityPlayback>() != null)
        {
            GameObject.FindObjectOfType<HighQualityPlayback>().PlayYoutubeVideo("bc0sJvtKrRM");
            GameObject.FindObjectOfType<HighQualityPlayback>().unityVideoPlayer.loopPointReached += OnVideoFinished;
        }
        mainUI.SetActive(false);
    }

    public UnityEngine.UI.Text videoUrlInput;

    public void PlayFromInput()
    {
        //search for the low quality if not find search for highquality
        if (GameObject.FindObjectOfType<SimplePlayback>() != null)
        {
            GameObject.FindObjectOfType<SimplePlayback>().PlayYoutubeVideo(videoUrlInput.text);
            GameObject.FindObjectOfType<SimplePlayback>().unityVideoPlayer.loopPointReached += OnVideoFinished;
        }
        else if (GameObject.FindObjectOfType<HighQualityPlayback>() != null)
        {
            GameObject.FindObjectOfType<HighQualityPlayback>().PlayYoutubeVideo(videoUrlInput.text);
            GameObject.FindObjectOfType<HighQualityPlayback>().unityVideoPlayer.loopPointReached += OnVideoFinished;
        }
        mainUI.SetActive(false);
    }

    private void OnVideoFinished(VideoPlayer vPlayer)
    {
        if (GameObject.FindObjectOfType<SimplePlayback>() != null)
        {
            GameObject.FindObjectOfType<SimplePlayback>().unityVideoPlayer.loopPointReached -= OnVideoFinished;
        }
        else if (GameObject.FindObjectOfType<HighQualityPlayback>() != null)
        {
            GameObject.FindObjectOfType<HighQualityPlayback>().unityVideoPlayer.loopPointReached -= OnVideoFinished;
        }
        mainUI.SetActive(true);
    }
}
