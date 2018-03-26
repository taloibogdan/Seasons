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

    private float m_fInvincibilityCooldownMax = 1;
    private float m_fInvincibilityCooldown = -1;
    private float m_fInvinciBlinkLong = 0.2f;
    private float m_fInvinciBlinkShort = 0.1f;

    private float m_fProjectileCooldownMax = 2;
    private float m_fProjectileCooldown = -1;

    private float HarpoonCooldownMax = 1;
    private float HarpoonCooldown = -1;
    private float HarpoonSpeed = 10;
    private Transform HarpoonTarget = null;

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

        //STRING
        if (HarpoonCooldown > 0)
        {
            HarpoonCooldown -= Time.deltaTime;
        }
        if (Input.GetMouseButtonDown(1) && HarpoonCooldown < 0)
        {
            m_rigidbody.useGravity = false;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, -GameManager.GetInstance().Camera.transform.position.z * 2, 1 << 9))
            {
                HarpoonCooldown = HarpoonCooldownMax;
                Debug.Log(hit.transform.tag);
                HarpoonTarget = hit.transform;
            }
        }
        if (Input.GetMouseButton(1) && HarpoonTarget != null)
        {
            float step = HarpoonSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, HarpoonTarget.position, step);
            Debug.DrawRay(transform.position, HarpoonTarget.position - transform.position, Color.white);
            Debug.Log(transform.position);
            if(Vector3.Distance(transform.position, HarpoonTarget.position) < 1)
            {
                HarpoonTarget = null;
                m_rigidbody.useGravity = true;
            }
        }
        if (Input.GetMouseButtonUp(1))
        {
            HarpoonTarget = null;
            m_rigidbody.useGravity = true;
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

