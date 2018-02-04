using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nav : MonoBehaviour
{
    public static Nav instance;

    void Awake()
    {
        instance = this;
    }
	void Start ()
    {
		
	}
}
