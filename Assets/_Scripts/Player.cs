using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour {

    public float ForceMultiplier = 10000;
    public float JumpForce = 4000;
    public float JumpTime = 1.5f;
    public float JumpExtensionForce = 200;
    public int MaxHP = 3;
	public int HP = 3;

	// DASH
	public float dashBoost = 20f;
	public float sinusUnits = 20f;
	public float sinusMaxDegrees = 180f;
	private bool isDashing = false;
	private float incrementalDegreesSinus = 0f;
	// DASH

    private float m_fInvincibilityCooldownMax = 1;
    private float m_fInvincibilityCooldown = -1;
    private float m_fInvinciBlinkLong = 0.2f;
    private float m_fInvinciBlinkShort = 0.1f;

    private float m_fProjectileCooldownMax = 3;
    private float m_fProjectileCooldown = -1;

    public float HookCooldownMax = 1;
    public float HookHeadSpeed = 10;
    public float HookSpeed = 4;
    public float HookLength = 6;
    private float m_fHookCooldown = -1;
    private Hook m_hook = null;
    private bool m_isHookActive = false;
    private Vector3 m_vHookCollisionPoint;

    private Renderer m_renderer;
    private Rigidbody m_rigidbody;
    private float m_fJumpStartTime = 0;
    private int m_nJumpCharges = 0;
    private int m_nMaxJumpCharges = 2;

    private GameManager m_gameManager;
    private ResourceManager m_resourceManager;
	private UIManager m_uiManager;

    void Start() {
        m_renderer = transform.GetComponent<Renderer>();
        m_rigidbody = transform.GetComponent<Rigidbody>();
        m_gameManager = GameManager.GetInstance();
        m_gameManager.GameRunning = true;
        m_resourceManager = ResourceManager.GetInstance();
		m_uiManager = UIManager.GetInstance();

		// UI
		m_uiManager.ShootingCooldown.gameObject.SetActive (false);
		m_uiManager.HealthStats.text = "" + MaxHP;
		m_uiManager.EssenceStats.text = "" + m_resourceManager.GetEssence();
    }

    void Update()
    {
        //MOVEMENT
        if (m_gameManager.GameRunning == false)
        {
            m_rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            return;
        }
        else m_rigidbody.constraints = RigidbodyConstraints.FreezePositionZ;
        m_rigidbody.AddForce(new Vector3((Input.GetAxis("Horizontal") - (m_rigidbody.velocity.x / 4)) * Time.deltaTime * ForceMultiplier, 0, 0));

        if (IsGrounded()) m_nJumpCharges = m_nMaxJumpCharges;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (m_nJumpCharges > 0)
            {
                --m_nJumpCharges;
                m_rigidbody.velocity = new Vector3(m_rigidbody.velocity.x, 0, 0);
                m_rigidbody.AddForce(new Vector3(0, JumpForce, 0));
                m_fJumpStartTime = Time.time;
            }
        }
        if (Input.GetKey(KeyCode.Space) && Time.time - m_fJumpStartTime < JumpTime)
        {
            m_rigidbody.AddForce(new Vector3(0, (JumpTime - (Time.time - m_fJumpStartTime)) * Time.deltaTime * JumpExtensionForce, 0));
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
        if (m_fHookCooldown > 0)
        {
            m_fHookCooldown -= Time.deltaTime;
        }
        if (Input.GetMouseButtonDown(1) && m_fHookCooldown < 0 && !m_isHookActive)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, -GameManager.GetInstance().Camera.transform.position.z * 2, 1 << 8))
            {
				Debug.Log(hit.point);
                m_hook = Instantiate(m_resourceManager.Hook, transform.position, Quaternion.Euler(0, 0, 0)).GetComponent<Hook>();
                m_hook.Init((hit.point - transform.position).normalized * HookHeadSpeed, HookLength);
                m_fHookCooldown = 99999;
            }
        }
        if (Input.GetMouseButton(1) && m_isHookActive)
        {
            float step = HookSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, m_vHookCollisionPoint, step);
            if(Vector3.Distance(transform.position, m_vHookCollisionPoint) < .5)
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
            }
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

		if (isDashing) 
		{
			incrementalDegreesSinus += sinusUnits;
			float cornerAngle = incrementalDegreesSinus * Mathf.PI / 180f;

			m_rigidbody.AddForce(movement * dashBoost * Mathf.Sin(cornerAngle), ForceMode.Impulse);

			if (incrementalDegreesSinus >= sinusMaxDegrees) {
				isDashing = false;
				incrementalDegreesSinus = 0f;
			}
		}

		if (Input.GetKeyDown (KeyCode.LeftShift)) {
			incrementalDegreesSinus = 0f;
			isDashing = true;
		}
	}

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, transform.localScale.y / 2.0f + 0.01f);
    }

    void OnTriggerStay(Collider other)
    {
        if(other.tag == "Trap")
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

