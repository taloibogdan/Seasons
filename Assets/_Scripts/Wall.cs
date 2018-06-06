using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour {

    public bool isVisible;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnBecameVisible()
    {
        Debug.Log("Wall is visible");
        isVisible = true;
    }

    void OnBecameInvisible()
    {
        isVisible = false;
    }
}
