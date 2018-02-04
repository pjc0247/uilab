using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class VideoController : MonoBehaviour
{
    private bool noHD;

    public VideoPlayer sourceVideo;
    public VideoPlayer sourceAudioVideo;
    public bool hideControls = false;
    public int secondsToHideScreen = 3;
    [Header("The main controller ui parent")]
    public GameObject mainControllerUi;
    [Header("Slider with duration and progress")]
    public Slider progressSlider;
    [Header("Play/Pause Button")]
    public GameObject playImage;
    [Header("Volume slider")]
    public Slider volumeSlider;
    [Header("Playback speed")]
    public Slider playbackSpeed;
    [Header("Current Time")]
    public Text currentTimeString;
    [Header("Total Time")]
    public Text totalTimeString;

    private float totalVideoDuration;
    private float currentVideoDuration;

    private bool videoSeekDone = false;
    private bool videoAudioSeekDone = false;

    [Header("Loading control")]
    public GameObject loadingPanel;
    public Text loadingMessage;


    private void Start()
    {
        if(sourceVideo.GetComponent<HighQualityPlayback>()!= null)
            noHD = sourceVideo.GetComponent<HighQualityPlayback>().noHD;
        else
        {
            noHD = false;
        }
        //Remove the audio if use the hd playback (audio play separeted)
        if (!noHD)
            sourceVideo.audioOutputMode = VideoAudioOutputMode.None;
    }

    public void ShowLoading(string message)
    {
        Debug.Log("Loading: " + message);
        if(loadingPanel != null)
        {
            loadingPanel.SetActive(true);
            loadingMessage.text = message;
        }
    }

    public void HideLoading()
    {
        if(loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    private float hideScreenTime = 0;

    void Update()
    {
        if (sourceVideo.isPlaying && progressSlider != null)
        {
            totalVideoDuration = Mathf.RoundToInt(sourceVideo.frameCount / sourceVideo.frameRate);
            currentVideoDuration = Mathf.RoundToInt(sourceVideo.frame / sourceVideo.frameRate);
            progressSlider.maxValue = totalVideoDuration;
            progressSlider.Set(currentVideoDuration);
        }

        if (currentVideoDuration >= totalVideoDuration)
        {
            if(!finished)
                Finished();
        }

        if (hideControls)
        {
            if (UserInteract())
            {
                hideScreenTime = 0;
                if(mainControllerUi != null)
                    mainControllerUi.SetActive(true);
            }
            else
            {
                hideScreenTime += Time.deltaTime;
                if (hideScreenTime >= secondsToHideScreen)
                {
                    //not increment
                    hideScreenTime = secondsToHideScreen;
                    HideControllers();
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if(currentTimeString != null && totalTimeString != null)
        {
            currentTimeString.text = FormatTime(Mathf.RoundToInt(currentVideoDuration));
            totalTimeString.text = FormatTime(Mathf.RoundToInt(totalVideoDuration));
        }
    }

    private void HideControllers()
    {
        if(mainControllerUi != null)
        {
            mainControllerUi.SetActive(false);
            showingVolume = false;
            showingPlaybackSpeed = false;
            volumeSlider.gameObject.SetActive(false);
            playbackSpeed.gameObject.SetActive(false);
        }
    }


    private bool finished = false;
    public void Seek()
    {
        sourceVideo.GetComponent<HighQualityPlayback>().isSyncing = true;
        //check if can seek
		if (Mathf.RoundToInt(currentVideoDuration) != Mathf.RoundToInt(totalVideoDuration))
        {
            finished = false;
            if (sourceVideo.canStep && sourceVideo.canSetTime)
            {
                ShowLoading("Syncing...");
                //Pause the video to prevent audio error
                Pause();
                //reset variables
                videoSeekDone = false;
                videoAudioSeekDone = false;
                if (!noHD)
                {
                    //callbacks
                    sourceAudioVideo.seekCompleted += SeekVideoAudioDone;
                    //change the time
                    if(Mathf.RoundToInt(progressSlider.value) == 0)
                        sourceAudioVideo.time = 1;
                    else
                        sourceAudioVideo.time = Mathf.RoundToInt(progressSlider.value);
                }
                else
                {
                    //callback
                    sourceVideo.seekCompleted += SeekVideoDone;
                    if (Mathf.RoundToInt(progressSlider.value) == 0)
                        sourceVideo.time = 1;
                    else
                        sourceVideo.time = Mathf.RoundToInt(progressSlider.value);
                }
            }
        }
        else
        {
//			if (sourceVideo.isPlaying) {
//				if (!finished)
//					Finished();
//			}
        }
    }

    public void Finished()
    {
        finished = true;
        if(sourceVideo.GetComponent<HighQualityPlayback>()  != null)
            sourceVideo.GetComponent<HighQualityPlayback>().OnVideoFinished();
        else
        {
            if (sourceVideo.GetComponent<WebGlPlayback>() != null)
                sourceVideo.GetComponent<WebGlPlayback>().OnVideoFinished();
        }
    }

    public void Volume()
    {
        if (sourceVideo.audioOutputMode == VideoAudioOutputMode.Direct)
            sourceAudioVideo.SetDirectAudioVolume(0, volumeSlider.value);
        else if (sourceVideo.audioOutputMode == VideoAudioOutputMode.AudioSource)
            sourceVideo.GetComponent<AudioSource>().volume = volumeSlider.value;
        else
            sourceVideo.GetComponent<AudioSource>().volume = volumeSlider.value;
    }

    public void Speed()
    {
        if (sourceVideo.canSetPlaybackSpeed)
        {
            if (playbackSpeed.value == 0)
            {
                sourceVideo.playbackSpeed = 0.5f;
                sourceAudioVideo.playbackSpeed = 0.5f;
            }
            else
            {
                sourceVideo.playbackSpeed = playbackSpeed.value;
                sourceAudioVideo.playbackSpeed = playbackSpeed.value;
            }
        }
    }

    public void PlayButton()
    {
        if (!sourceVideo.isPlaying)
        {
            playImage.SetActive(false);
            sourceAudioVideo.time = sourceVideo.time;
            sourceAudioVideo.frame = sourceVideo.frame;
            Play();
        }
        else
        {
            playImage.SetActive(true);
            Pause();
        }

    }
    private bool showingPlaybackSpeed = false;
    private bool showingVolume = false;

    public void VolumeSlider()
    {
        if (showingVolume)
        {
            showingVolume = false;
            volumeSlider.gameObject.SetActive(false);
        }
        else
        {
            showingVolume = true;
            volumeSlider.gameObject.SetActive(true);
        }
    }

    public void PlaybackSpeedSlider()
    {
        if (showingPlaybackSpeed)
        {
            showingPlaybackSpeed = false;
            playbackSpeed.gameObject.SetActive(false);
        }
        else
        {
            showingPlaybackSpeed = true;
            playbackSpeed.gameObject.SetActive(true);
        }
    }

    public void Pause()
    {
        sourceVideo.Pause();
        if (!noHD)
            sourceAudioVideo.Pause();
    }

    public void Play()
    {
        StartCoroutine(WaitAndPlay());
    }

    IEnumerator WaitAndPlay()
    {
        if (!noHD)
        {
            sourceAudioVideo.Play();
            yield return new WaitForSeconds(0.35f);
        }else
            yield return new WaitForSeconds(1f);//if is no hd wait some more

        sourceVideo.Play();
    }

    public void Stop()
    {
        sourceVideo.Stop();
        if (!noHD)
            sourceAudioVideo.Stop();
    }

    void SeekVideoDone(VideoPlayer vp)
    {
        sourceVideo.seekCompleted -= SeekVideoDone;
        videoSeekDone = true;
        if (!noHD)
        {
            //check if the two videos are done the seek | if are play the videos
            if (videoSeekDone && videoAudioSeekDone)
            {
                sourceVideo.GetComponent<HighQualityPlayback>().isSyncing = false;
                long frame = sourceVideo.frame;
                sourceVideo.frame = frame;
                sourceAudioVideo.frame = frame;
                StartCoroutine(SeekFinished());
                
                //HideLoading();
                //Play();
            }
        }
        else
        {
            sourceVideo.GetComponent<HighQualityPlayback>().isSyncing = false;
            HideLoading();
            Play();
        }
    }

    void SeekVideoAudioDone(VideoPlayer vp)
    {
        sourceAudioVideo.seekCompleted -= SeekVideoAudioDone;
        sourceVideo.seekCompleted += SeekVideoDone;
        sourceVideo.frame = sourceAudioVideo.frame;
        sourceVideo.time = sourceAudioVideo.time;
        videoAudioSeekDone = true;
    }

    IEnumerator SeekFinished()
    {
        yield return new WaitForSeconds(1);
        HideLoading();
        Play();
    }

    private string FormatTime(int time)
    {
        int hours = time / 3600;
        int minutes = (time % 3600) / 60;
        int seconds = (time % 3600) % 60;
        if (hours == 0 && minutes != 0)
        {
            return minutes.ToString("00") + ":" + seconds.ToString("00");
        }
        else if (hours == 0 && minutes == 0)
        {
            return "00:" + seconds.ToString("00");
        }
        else
        {
            return hours.ToString("00") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00");
        }
    }

    bool UserInteract()
    {
        if (Application.isMobilePlatform)
        {
            if (Input.touches.Length >= 1)
                return true;
            else
                return false;
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
                return true;
            return (Input.GetAxis("Mouse X") != 0) || (Input.GetAxis("Mouse Y") != 0);
        }

    }
}
