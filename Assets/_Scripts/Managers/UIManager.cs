using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
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

	private static UIManager instance;
	public static UIManager GetInstance()
	{
		return instance;
	}
	#endregion
	public Image ShootingCooldown;
	public Text HealthStats;
	public Text EssenceStats;
}