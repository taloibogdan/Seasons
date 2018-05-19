using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prompt : MonoBehaviour {

    public GameObject prompt;
    private bool m_isActive = false;
	// Use this for initialization
	void Start () {
        prompt.SetActive(false);	
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.tag.Contains("Player") && m_isActive == false)
        {
            prompt.SetActive(true);
            m_isActive = true;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.tag.Contains("Player") && m_isActive)
        {
            prompt.SetActive(false);
            m_isActive = false;
        }
    }
}
