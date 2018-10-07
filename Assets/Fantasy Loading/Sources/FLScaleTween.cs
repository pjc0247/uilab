using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FLScaleTween : MonoBehaviour {
	public float delay = 0;
    public Vector3 scaleSize;
    public float animatTime = 0.5f;
	// Use this for initialization
	IEnumerator Start () {
		yield return new WaitForSeconds(delay);
        iTween.ScaleTo(gameObject,
            iTween.Hash(
            "scale", scaleSize,
            "islocal", true,
            "looptype", iTween.LoopType.pingPong,
            "easetype", iTween.EaseType.easeInSine,
            "time", animatTime
            ));
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
