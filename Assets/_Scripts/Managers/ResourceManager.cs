using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour {
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

    private static ResourceManager instance;
    public static ResourceManager GetInstance()
    {
        return instance;
    }
    #endregion
    public GameObject PlayerProjectile;
    public GameObject Hook;
}
