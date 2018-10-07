using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FLRotateSkip : MonoBehaviour
{
    public float skipTime = 0.1f;
    public float skipAngle = 45;
	// Use this for initialization
	IEnumerator Start () {
        while (true)
        {
            yield return new WaitForSeconds(skipTime);
            transform.Rotate(-Vector3.forward * skipAngle);
        }
	}
	
}
