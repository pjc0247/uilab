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
            "This is a my portfolio page which imitates AppStore.\r\n" +
            "\r\n" +
            "Just pull down from top of the screen If you want to return to previous page.";
    }
    void OnShrinkContent()
    {
        player.Stop();

        description.text =
            "Let me introduce what is it.";
    }
}
