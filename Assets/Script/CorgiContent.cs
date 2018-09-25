using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CorgiContent : MonoBehaviour
{
    public VideoPlayer player;
    public Text description;

	void OnExpandContent()
    {
        player.Play();

        description.text =
            "Anime style action FPS.\r\n" +
            "Made with Unity.";
    }
    void OnShrinkContent()
    {
        player.Stop();

        description.text =
            "Anime action FPS game.";
    }
}
