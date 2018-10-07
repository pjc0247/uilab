using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FLRotate : MonoBehaviour {
    public float rotateSpeed = 300;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(-Vector3.forward * rotateSpeed * Time.deltaTime);
	}
}
