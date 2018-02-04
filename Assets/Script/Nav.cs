using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nav : MonoBehaviour
{
    public enum Scene
    {
        AppDetail
    }

    public static Nav instance;

    public RectTransform appDetail;

    void Awake()
    {
        instance = this;
    }
	void Start ()
    {
		
	}

    public void OnClickGameProjects()
    {
        Push(Scene.AppDetail);
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
}
