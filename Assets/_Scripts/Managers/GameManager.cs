using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    #region Singleton
    void Awake()
    {
        if (instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
    }
    
    private static GameManager instance;
    public static GameManager GetInstance()
    {
        return instance;
    }
    #endregion

    public bool GameRunning = true;
    public GameObject Player = null;
    public FollowCamera Camera = null;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
