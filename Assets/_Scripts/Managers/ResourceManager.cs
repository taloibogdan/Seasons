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
    public GameObject EnemyProjectile;
    public GameObject SlowProjectile;
    public GameObject Hook;
    public GameObject WallRight;
	private UIManager m_uiManager;

    private int Essence = 0;

	void Start()
	{
		m_uiManager = UIManager.GetInstance();
	}
    public void AddEssence(int GainedEssence)
    {
        Essence += GainedEssence;
        Debug.Log("Essence: " + Essence);
		m_uiManager.EssenceStats.text = "" + GetEssence();
    }

    public int GetEssence()
    {
        return Essence;
    }
}
