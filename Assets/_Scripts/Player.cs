using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private float m_fProjectileCooldownMax = 2;
    private float m_fProjectileCooldown = -1;

    private float HookCooldownMax = 1;
    private float HookCooldown = -1;
    private float HookSpeed = 10;
    private bool HookIsActive = false;
    private Vector3 HookCollisionPoint;

    private Renderer m_renderer;
    private Rigidbody m_rigidbody;
    private float m_fJumpStartTime = 0;
    private int m_nJumpCharges = 0;
    private int m_nMaxJumpCharges = 2;

    private GameManager m_gameManager;
    private ResourceManager m_resourceManager;

    void Start() {
        m_renderer = transform.GetComponent<Renderer>();
        m_rigidbody = transform.GetComponent<Rigidbody>();
        m_gameManager = GameManager.GetInstance();
        m_gameManager.GameRunning = true;
        m_resourceManager = ResourceManager.GetInstance();
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
        if (m_fProjectileCooldown > 0)
        {
            m_fProjectileCooldown -= Time.deltaTime;
        }
        if (Input.GetMouseButtonDown(0) && m_fProjectileCooldown < 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, -GameManager.GetInstance().Camera.transform.position.z * 2, 1 << 8))
            {
                m_fProjectileCooldown = m_fProjectileCooldownMax;
                Debug.Log(hit.point);
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
        if (HookCooldown > 0)
        {
            HookCooldown -= Time.deltaTime;
        }
        if (Input.GetMouseButtonDown(1) && HookCooldown < 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, -GameManager.GetInstance().Camera.transform.position.z * 2, 1 << 9))
            {
                if(hit.transform.tag.Equals("Platform"))
                {
                    Debug.Log(hit.point);
                    m_rigidbody.useGravity = false;
                    HookCooldown = HookCooldownMax;
                    HookIsActive = true;
                    HookCollisionPoint = hit.point;
                    //HookTarget = hit.transform;
                }
            }
        }
        if (Input.GetMouseButton(1) && HookIsActive)
        {
            float step = HookSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, HookCollisionPoint, step);
            //Debug.DrawRay(transform.position, HookCollisionPoint, Color.white);
            if(Vector3.Distance(transform.position, HookCollisionPoint) < 1)
            {
                HookIsActive = false;
                m_rigidbody.useGravity = true;
            }
        }
        if (Input.GetMouseButtonUp(1))
        {
            //HookTarget = null;
            HookIsActive = false;
            m_rigidbody.useGravity = true;
        }
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

    void OnTriggerEnter(Collider other)
    {
        
    }
    public void GetDamaged()
    {
        if(m_fInvincibilityCooldown > 0)
        {
            return;
        }
        m_fInvincibilityCooldown = m_fInvincibilityCooldownMax;
        HP--;
        if(HP <= 0)
        {
            Die();
        }
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

