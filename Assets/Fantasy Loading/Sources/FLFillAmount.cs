using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FLFillAmount : MonoBehaviour {
    
    private Image targetImage;
	// Use this for initialization
	void Start () {
        targetImage = GetComponent<Image>();
	}
	
	// Update is called once per frame
    void Update()
    {
        float angle = transform.eulerAngles.z;
        targetImage.fillAmount = Mathf.Abs( angle / 360-0.5f);
        
	}
}
