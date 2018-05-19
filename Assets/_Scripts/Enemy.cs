using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float HP = 2;
    public float Speed = 2;
    public float Range = 3;
    public float AggroRange = 8;
    public bool IsFlying = false;
    public bool IsRanged = false;
    public GameObject Drop;

    private float m_fInvincibilityCooldownMax = 1;
    private float m_fInvincibilityCooldown = -1;
    private float m_fInvinciBlinkLong = 0.2f;
    private float m_fInvinciBlinkShort = 0.1f;

    private float m_fProjectileCooldownMax = 2;
    private float m_fProjectileCooldown = -1;

    private Renderer m_renderer;
    private Rigidbody m_rigidbody;
    private GameObject m_player;
    private GameManager m_gameManager;
    private ResourceManager m_resourceManager;

    void Start()
    {
        m_renderer = transform.GetComponent<Renderer>();
        m_rigidbody = transform.GetComponent<Rigidbody>();
        m_gameManager = GameManager.GetInstance();
        m_resourceManager = ResourceManager.GetInstance();
        m_player = m_gameManager.Player;

        if(IsFlying)
        {
            m_rigidbody.useGravity = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_gameManager.GameRunning == false)
        {
            return;
        }

        Vector3 pos = m_player.transform.position;
        float dx = pos.x - transform.position.x;
        if (Mathf.Abs(dx) > AggroRange)
        {
            return;
        }
        float sgn = dx / Mathf.Abs(dx);
        if (dx * sgn > Speed * Time.deltaTime && Range < (pos - transform.position).magnitude)
        {
            transform.position += new Vector3(Speed * Time.deltaTime * sgn, 0, 0);
            if (IsFlying)
            {

            }
        }

        //PROJECTILES
        if(IsRanged)
        {
            if (m_fProjectileCooldown > 0)
            {
                m_fProjectileCooldown -= Time.deltaTime;
            }
            else
            {
                if(Range >= (pos - transform.position).magnitude)
                {
                    m_fProjectileCooldown = m_fProjectileCooldownMax;
                    //Debug.Log(hit.point);
                    Projectile proj = Instantiate(m_resourceManager.EnemyProjectile, transform.position, Quaternion.Euler(0, 0, 0)).GetComponent<Projectile>();
                    proj.SetTarget(pos);
                }
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
    }

    void FixedUpdate()
    {
        if (IsFlying)
        {
            transform.position = Vector3.Lerp(transform.position + Vector3.up/50, transform.position - Vector3.up/50, Mathf.PingPong(Time.time, 1));
        }
    }

    public void GetDamaged()
    {
        if (m_fInvincibilityCooldown > 0)
        {
            return;
        }
        m_fInvincibilityCooldown = m_fInvincibilityCooldownMax;
        HP--;
        if (HP <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Instantiate(Drop, transform.position, new Quaternion(0, 0, 0, 0));
        Drop.GetComponent<Essence>().SetEnemyTag(transform.tag);
        Destroy(gameObject);
    }
    void OnTriggerStay(Collider c)
    {
        if (c.transform.tag.Equals("PlayerCollider"))
        {
            c.transform.parent.GetComponent<Player>().GetDamaged();
        }
        if (c.tag == "Trap")
        {
            GetDamaged();
        }
    }
}
