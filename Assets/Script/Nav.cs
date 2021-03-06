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
        Profile,
        Setting
    }

    public static Nav instance;

    public RectTransform feed;
    public RectTransform appDetail;
    public RectTransform appList;
    public RectTransform profile;
    public RectTransform setting;

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
        Change(Scene.AppList);
    }
    public void OnClickProfile()
    {
        Change(Scene.Profile);
    }
    public void OnClickUtilityProjects()
    {
        Change(Scene.Setting);
    }

    public void Change(Scene scene)
    {
        Pop();
        if (scene == Scene.AppList)
        {
            StartCoroutine(SlideToFunc(-1218, feed));
            StartCoroutine(SlideToFunc(0, appList));
            StartCoroutine(SlideToFunc(1218, profile));
            StartCoroutine(SlideToFunc(1218, setting));
        }
        else if (scene == Scene.Feed)
        {
            StartCoroutine(SlideToFunc(0, feed));
            StartCoroutine(SlideToFunc(1218, appList));
            StartCoroutine(SlideToFunc(1218, profile));
            StartCoroutine(SlideToFunc(1218, setting));
        }
        else if (scene == Scene.Profile)
        {
            StartCoroutine(SlideToFunc(0, profile));
            StartCoroutine(SlideToFunc(1218, feed));
            StartCoroutine(SlideToFunc(1218, appList));
            StartCoroutine(SlideToFunc(1218, setting));
        }
        else if (scene == Scene.Setting)
        {
            StartCoroutine(SlideToFunc(0, setting));
            StartCoroutine(SlideToFunc(1218, profile));
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
        AppDetailController.pageName = name;

        appDetail.gameObject.SetActive(false);
        appDetail.gameObject.SetActive(true);
        Push(Scene.AppDetail);
    }
}
