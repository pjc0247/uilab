using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode] 
public class RadialProgressButton : MonoBehaviour
{
    public Image progress;
    public Image fill;

    public float value;

    void Awake()
    {
        progress = progress.AssignIfNull(this, "Progress");
        fill = fill.AssignIfNull(this, "Fill");
    }
	void Start ()
    {
		
	}

    void OnValidate()
    {
        progress.fillAmount = Mathf.Clamp(value, 0, 1);
    }
}

