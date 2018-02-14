using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class UiState : MonoBehaviour
{
    private class AnimationList
    {
        public List<Coroutine> coroutines;
        public int refCount;

        public AnimationList()
        {
            coroutines = new List<Coroutine>();
        }
    }

    private Dictionary<string, AnimationList> animations =
        new Dictionary<string, AnimationList>();

	void Start ()
    {
	}

    public bool IsPlaying(string key)
    {
        if (animations.ContainsKey(key) == false)
            return false;

        return animations[key].refCount != 0;
    }

    public Coroutine StartAnimation(string key, IEnumerator coro)
    {
        if (string.IsNullOrEmpty(key)) throw new ArgumentException("key");
        if (animations.ContainsKey(key) == false)
            animations[key] = new AnimationList();

        var c = StartCoroutine(WrapCoroutine(key, coro));
        animations[key].coroutines.Add(c);

        return c;
    }
    private IEnumerator WrapCoroutine(string key, IEnumerator coro)
    {
        animations[key].refCount++;
        yield return StartCoroutine(coro);
        animations[key].refCount--;
    }
    public void ForceStopAllAnimations()
    {
        StopAllCoroutines();

        foreach (var anim in animations)
        {
            anim.Value.coroutines.Clear();
            anim.Value.refCount = 0;
        }
    }
    public void ForceStopAnimation(string key)
    {
        if (string.IsNullOrEmpty(key)) throw new ArgumentException("key");
        if (animations.ContainsKey(key) == false)
            return;

        foreach (var coro in animations[key].coroutines)
            StopCoroutine(coro);

        animations[key].coroutines.Clear();
        animations[key].refCount = 0;
    }
}
