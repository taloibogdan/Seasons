﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour {

    public float ForceMultiplier = 3000;
    public float JumpForce = 320;
    public float JumpTime = 1.5f;
    public float JumpExtensionForce = 100;
    public int MaxHP = 3;
	public int HP = 3;

	// DASH
	public float dashBoost = 10f;
	public float sinusUnits = 20f;
	public float sinusMaxDegrees = 180f;
	private bool isDashing = false;
	private float incrementalDegreesSinus = 0f;
	private float m_fDashCooldown = -1;
	private float m_fDashCooldownMax = 2;
	private int maxDashes = 2;
	private int numberOfDashes = 0;
	// DASH

    private float m_fInvincibilityCooldownMax = 1;
    private float m_fInvincibilityCooldown = -1;
    private float m_fInvinciBlinkLong = 0.2f;
    private float m_fInvinciBlinkShort = 0.1f;

    private float m_fProjectileCooldownMax = 1.3f;
    private float m_fProjectileCooldown = -1;

    private float m_fSlowFactor = 1.0f;
    private float m_fSlowTime = 10;
    private float m_fSlowTimeMax = 5;

    public float HookCooldownMax = 4;
    public float HookHeadSpeed = 12;
    public float HookSpeed = 6;
    public float HookLength = 6;
    private float m_fHookCooldown = -1;
    private Hook m_hook = null;
    private bool m_isHookActive = false;
    private Vector3 m_vHookCollisionPoint;

    private Vector3 playerPos;
    private Renderer m_renderer;
    private Rigidbody m_rigidbody;
    private ParticleSystem m_slowParticles;
    private float m_fJumpStartTime = 0;
    private int m_nJumpCharges = 0;
    private int m_nMaxJumpCharges = 2;

    private GameManager m_gameManager;
    private ResourceManager m_resourceManager;
	private UIManager m_uiManager;

    void Start() {
        m_renderer = transform.GetComponentInChildren<MeshRenderer>();
        m_rigidbody = transform.GetComponent<Rigidbody>();
        m_slowParticles = transform.GetComponentInChildren<ParticleSystem>();
        m_slowParticles.Stop();
        m_gameManager = GameManager.GetInstance();
        m_gameManager.GameRunning = true;
        m_resourceManager = ResourceManager.GetInstance();
		m_uiManager = UIManager.GetInstance();

		// UI
		m_uiManager.ShootingCooldown.gameObject.SetActive (false);
		m_uiManager.HookCooldown.gameObject.SetActive (false);
		m_uiManager.DashCooldown.gameObject.SetActive (false);
		m_uiManager.HealthStats.text = "" + MaxHP;
		m_uiManager.EssenceStats.text = "" + m_resourceManager.GetEssence();

        playerPos = transform.position;
    }
    void Update()
    {
        //MOVEMENT
        if (m_gameManager.GameRunning == false)
        {
            m_rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            return;
        }
        else m_rigidbody.constraints = RigidbodyConstraints.FreezePositionZ|RigidbodyConstraints.FreezeRotation;
        m_rigidbody.AddForce(new Vector3((Input.GetAxis("Horizontal") - (m_rigidbody.velocity.x / 4)) * Time.deltaTime * ForceMultiplier * m_fSlowFactor, 0, 0));

        if(transform.position.x < playerPos.x)
        {
            Quaternion rot = transform.rotation;
            rot.y = 180;
            transform.rotation = rot;
        }
        
        if(transform.position.x > playerPos.x)
        {
            Quaternion rot = transform.rotation;
            rot.y = 0;
            transform.rotation = rot;
        }
        playerPos = transform.position;

        if (IsGrounded()) m_nJumpCharges = m_nMaxJumpCharges;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (m_nJumpCharges > 0)
            {
                --m_nJumpCharges;
                m_rigidbody.velocity = new Vector3(m_rigidbody.velocity.x, 0, 0);
                m_rigidbody.AddForce(new Vector3(0, JumpForce * m_fSlowFactor, 0));
                m_fJumpStartTime = Time.time;
            }
        }
        if (Input.GetKey(KeyCode.Space) && Time.time - m_fJumpStartTime < JumpTime)
        {
            m_rigidbody.AddForce(new Vector3(0, (JumpTime - (Time.time - m_fJumpStartTime)) * Time.deltaTime * JumpExtensionForce * m_fSlowFactor, 0));
        }

        //SLOW EFFECT
        if(m_fSlowTime < m_fSlowTimeMax)
        {
            m_fSlowTime += Time.deltaTime;

            if(m_fSlowTime >= m_fSlowTimeMax)
            {
                Debug.Log("Stop slow");
                m_fSlowFactor = 1.0f;
                m_slowParticles.Stop();
            }
        }

        //PROJECTILES
		if (m_fProjectileCooldown > 0) {

			m_fProjectileCooldown -= Time.deltaTime;		


			// Shooting Cooldown
			if (m_uiManager.ShootingCooldown.IsActive ()) {
				m_uiManager.ShootingCooldown.GetComponentInChildren<Text> ().text = "" + (int)m_fProjectileCooldown;
			} else {
				m_uiManager.ShootingCooldown.gameObject.SetActive (true);
			}
		} else {
			// Shooting Cooldown
			m_uiManager.ShootingCooldown.gameObject.SetActive (false);
		}
        if (Input.GetMouseButtonDown(0) && m_fProjectileCooldown < 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, -GameManager.GetInstance().Camera.transform.position.z * 2, 1 << 8))
            {
                m_fProjectileCooldown = m_fProjectileCooldownMax;
                //Debug.Log(hit.point);
                Projectile proj = Instantiate(m_resourceManager.PlayerProjectile, transform.position, Quaternion.Euler(0, 0, 0)).GetComponent<Projectile>();
                proj.SetTarget(hit.point);
            }
        }

        //DMG INVINCIBILITY
        if (m_fInvincibilityCooldown > 0)
        {
            m_fInvincibilityCooldown -= Time.deltaTime;
            if (m_fInvincibilityCooldown % (m_fInvinciBlinkLong + m_fInvinciBlinkShort) > m_fInvinciBlinkLong)
            {
                m_renderer.enabled = false;
            }
            else
            {
                m_renderer.enabled = true;
            }

        }

        //HOOK
		if (m_fHookCooldown > 0) {
			m_fHookCooldown -= Time.deltaTime;

			// Shooting Cooldown
			if (m_uiManager.HookCooldown.IsActive ()) {
				m_uiManager.HookCooldown.GetComponentInChildren<Text> ().text = "" + (int)m_fHookCooldown;
			} else {
				m_uiManager.HookCooldown.gameObject.SetActive (true);
			}
		} else {
			// Shooting Cooldown
			m_uiManager.HookCooldown.gameObject.SetActive (false);

			if (Input.GetMouseButtonDown(1) && m_fHookCooldown < 0 && !m_isHookActive)
			{
                Debug.Log("hitheredo");
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit, -GameManager.GetInstance().Camera.transform.position.z * 2, 1 << 8))
				{
					Debug.Log(hit.point);
					m_hook = Instantiate(m_resourceManager.Hook, transform.position, Quaternion.Euler(0, 0, 0)).GetComponent<Hook>();
					m_hook.Init((hit.point - transform.position).normalized * HookHeadSpeed, HookLength);
					m_fHookCooldown = HookCooldownMax;
				}
			}
			if (Input.GetMouseButton(1) && m_isHookActive)
			{
				float step = HookSpeed * Time.deltaTime;
				transform.position = Vector3.MoveTowards(transform.position, m_vHookCollisionPoint, step);
				if(Vector3.Distance(transform.position, m_vHookCollisionPoint) < .7)
				{
					m_isHookActive = false;
				}
			}
			if (Input.GetMouseButtonUp(1))
			{
				m_isHookActive = false;
				if (m_hook != null)
				{
					m_hook.DestroySelf();
					m_fHookCooldown = HookCooldownMax;
				}
			}
		}

		//DASH
		if (m_fDashCooldown > 0) {
			//Debug.Log (m_fDashCooldown);
			m_fDashCooldown -= Time.deltaTime;

			// Shooting Cooldown
			if (m_uiManager.DashCooldown.IsActive ()) {
				m_uiManager.DashCooldown.GetComponentInChildren<Text> ().text = "" + (int)m_fDashCooldown;
			} else {
				m_uiManager.DashCooldown.gameObject.SetActive (true);
			}
		} else {
			// Shooting Cooldown
			m_uiManager.DashCooldown.gameObject.SetActive (false);
		}
    }

    public void SetHookCollisionPoint(Vector3 pos)
    {
        m_rigidbody.isKinematic = true;
        m_vHookCollisionPoint = pos;
        m_isHookActive = true;
    }

    public void HookDestroyed()
    {
        m_rigidbody.isKinematic = false;
        m_isHookActive = false;
        m_vHookCollisionPoint = Vector3.zero;
        m_fHookCooldown = HookCooldownMax;
        m_hook = null;
    }

    void FixedUpdate()
	{
		float moveHorizontal = Input.GetAxis("Horizontal");
		Vector3 movement = new Vector3(moveHorizontal, 0.0f, 0.0f);

		if (isDashing) {
			incrementalDegreesSinus += sinusUnits;
			float cornerAngle = incrementalDegreesSinus * Mathf.PI / 180f;

			m_rigidbody.AddForce (movement * dashBoost * Mathf.Sin (cornerAngle) * m_fSlowFactor, ForceMode.Impulse);

			if (incrementalDegreesSinus >= sinusMaxDegrees) {
				isDashing = false;
				incrementalDegreesSinus = 0f;
			}
		}

		if (Input.GetKeyDown (KeyCode.LeftShift) && m_fDashCooldown <= 0) {
			incrementalDegreesSinus = 0f;
			isDashing = true;
			numberOfDashes++;

			if (numberOfDashes == maxDashes) 
			{
				m_fDashCooldown = m_fDashCooldownMax;
				numberOfDashes = 0;
			}
		}
	}

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, transform.localScale.y / 2.0f + 0.01f);
    }

    void OnTriggerStay(Collider other)
    {
        if(other.tag == "Trap" || other.tag == "BossTrap")
        {
            GetDamaged();
        }
    }

    public void GetDamaged()
    {
        if(m_fInvincibilityCooldown > 0)
        {
            return;
        }
        m_fInvincibilityCooldown = m_fInvincibilityCooldownMax;
        HP--;
		// UI
		m_uiManager.HealthStats.text = "" + HP;
        if(HP <= 0)
        {
            Die();
        }
    }

    public void ApplySlowEffect()
    {
        if (m_fInvincibilityCooldown > 0)
        {
            return;
        }
        Debug.Log("Start slow");
        m_fInvincibilityCooldown = m_fInvincibilityCooldownMax;
        m_fSlowFactor = 0.7f;
        m_fSlowTime = 0;
        m_slowParticles.Play();
    }

    public void AddEssence(int GainedEssence)
    {
        m_resourceManager.AddEssence(GainedEssence);
    }

    public void Die()
    {
        m_gameManager.GameRunning = false;
        Invoke("RestartLvl", 2);
    }
    public void RestartLvl()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Destroy(gameObject);
    }
}

