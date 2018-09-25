﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nav : MonoBehaviour
{
    public enum Scene
    {
        Feed,
        AppList,
        AppDetail,
        Profile
    }

    public static Nav instance;

    public RectTransform feed;
    public RectTransform appDetail;
    public RectTransform appList;
    public RectTransform profile;

    void Awake()
    {
        instance = this;
    }
	void Start ()
    {
		
	}

    public void OnClickFeed()
    {
        Change(Scene.Feed);
    }
    public void OnClickGameProjects()
    {
        Push(Scene.AppDetail);
    }
    public void OnClickProfile()
    {
        Change(Scene.Profile);
    }
    public void OnClickUtilityProjects()
    {
        Change(Scene.AppList);
    }

    public void Change(Scene scene)
    {
        Pop();
        if (scene == Scene.AppList)
        {
            StartCoroutine(SlideToFunc(-1218, feed));
            StartCoroutine(SlideToFunc(0, appList));
            StartCoroutine(SlideToFunc(1218, profile));
        }
        else if (scene == Scene.Feed)
        {
            StartCoroutine(SlideToFunc(0, feed));
            StartCoroutine(SlideToFunc(1218, appList));
            StartCoroutine(SlideToFunc(1218, profile));
        }
        else if (scene == Scene.Profile)
        {
            StartCoroutine(SlideToFunc(0, profile));
            StartCoroutine(SlideToFunc(1218, feed));
            StartCoroutine(SlideToFunc(1218, appList));
        }
    }
    public void Push(Scene scene)
    {
        StartCoroutine(SlideToFunc(0, appDetail));
    }
    public void Pop()
    {
        StartCoroutine(SlideToFunc(1218, appDetail));
    }
    IEnumerator SlideToFunc(int x, RectTransform t)
    {
        for (int i = 0; i < 30; i++)
        {
            t.anchoredPosition += ( 
                new Vector2(x, t.anchoredPosition.y) - t.anchoredPosition) * 0.35f;
            yield return null;
        }
    }

    public void PushAppDetail(string name)
    {
        Push(Scene.AppDetail);
    }
}
