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
            "Corgi dog, dog corgi, corgi dog\r\n" +
            "Dog corgi, corgi dog, dog corgi licks floor\r\n" + 
            "Woof, Woof, Woof, Woof";
    }
    void OnShrinkContent()
    {
        player.Stop();

        description.text =
            "Corgi dog, dog corgi, corgi dog";
    }
}
