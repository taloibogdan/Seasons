using System;
using UnityEngine;

public enum BossState
{
    Idle,
    BasicAttackState,
    JumpAttackState,
    SlowAttackState,
    DashAttackState
}

public class Boss : MonoBehaviour {

    public float HP = 25;
    public float Speed = 2;
    public float AggroRange = 10;
    public bool IsFlying = false;

    private BossState state = BossState.Idle;
    private float m_fInvincibilityCooldownMax = 1;
    private float m_fInvincibilityCooldown = -1;

    private float m_fConsecutiveShots = 0;
    private float m_fConsecutiveShotsMax = 3;
    private float m_fProjectileCooldownMax = 3;
    private float m_fProjectileCooldown = -1;

    //private float m_fPeakJumpHeight = 10;
    //private Vector3 m_JumpPosition;
    //private float m_fJumpTime = 3;

    private float m_fHaloCharge = 0;
    private float m_fHaloChargeMax = 2;

    private float m_fDashCharge = 0;
    private float m_fDashChargeMax = 1.5f;

    private Rigidbody m_rigidbody;
    private GameObject m_player;
    private GameManager m_gameManager;
    private ResourceManager m_resourceManager;

    // Use this for initialization
    void Start () {
        m_rigidbody = transform.GetComponent<Rigidbody>();
        m_gameManager = GameManager.GetInstance();
        m_player = m_gameManager.Player;
        m_resourceManager = ResourceManager.GetInstance();

        if (IsFlying)
        {
            m_rigidbody.useGravity = false;
        }
    }

    void FixedUpdate()
    {
        if (IsFlying)
        {
            transform.position = Vector3.Lerp(transform.position + Vector3.up / 50, transform.position - Vector3.up / 50, Mathf.PingPong(Time.time, 1));
        }
    }

    // Update is called once per frame
    void Update ()
    {
        if (m_gameManager.GameRunning == false)
        {
            return;
        }

        Vector3 pos = m_player.transform.position;
        float dx = pos.x - transform.position.x;
        float sgn = dx / Mathf.Abs(dx);
        if (Mathf.Abs(dx) > AggroRange)
        {
            resetSkillVariables();
            return;
        }

        switch (state)
        {
            case BossState.Idle:
                {
                    Debug.Log("Basic Attack state");
                    state = BossState.BasicAttackState;
                    break;
                }
            case BossState.BasicAttackState:
                {
                    if (m_fConsecutiveShots < m_fConsecutiveShotsMax)
                    {
                        if (m_fProjectileCooldown > 0)
                        {
                            m_fProjectileCooldown -= Time.deltaTime;
                        }
                        else
                        {
                            m_fProjectileCooldown = m_fProjectileCooldownMax;
                            Projectile proj = Instantiate(m_resourceManager.EnemyProjectile, transform.position, Quaternion.Euler(0, 0, 0)).GetComponent<Projectile>();
                            proj.SetLifetime(4);
                            proj.SetTarget(pos);
                            m_fConsecutiveShots++;
                        }
                        return;
                    }
                    else
                    {
                        m_fConsecutiveShots = 0;
                        m_fProjectileCooldown = 0;
                        //Debug.Log("Jump Attack state");
                        //state = BossState.JumpAttackState;
                        //m_JumpPosition = m_player.transform.position;
                        //m_rigidbody.useGravity = false;
                        Debug.Log("Slow Attack state");
                        state = BossState.SlowAttackState;
                        Component bossHalo = GetComponent("Halo");
                        bossHalo.GetType().GetProperty("enabled").SetValue(bossHalo, true, null);
                    }
                    break;
                }
            case BossState.JumpAttackState:
                {
                    //float x0 = transform.position.x;
                    //float x1 = m_JumpPosition.x;
                    //float dist = x1 - x0;
                    //float nextX = Mathf.MoveTowards(transform.position.x, x1, Speed * Time.deltaTime);
                    //float baseY = Mathf.Lerp(transform.position.y, m_JumpPosition.y, (nextX - x0) / dist);
                    //float arc = m_fPeakJumpHeight * (nextX - x0) * (nextX - x1) / (-0.25f * dist * dist);
                    //Vector3 nextPos = new Vector3(nextX, baseY + arc, transform.position.z);

                    //// Rotate to face the next position, and then move there
                    //Vector3 forward = nextPos - transform.position;
                    //transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg);;
                    //transform.position = nextPos;

                    //// Do something when we reach the target
                    //if (nextPos == m_JumpPosition)
                    //{
                    //    state = BossState.Idle;
                    //}
                    break;
                }
            case BossState.SlowAttackState:
                {
                    if(m_fHaloCharge < m_fHaloChargeMax)
                    {
                        m_fHaloCharge += Time.deltaTime;
                    }
                    else
                    {
                        Component bossHalo = GetComponent("Halo");
                        bossHalo.GetType().GetProperty("enabled").SetValue(bossHalo, false, null);
                        Projectile proj = Instantiate(m_resourceManager.SlowProjectile, transform.position, Quaternion.Euler(0, 0, 0)).GetComponent<Projectile>();
                        proj.SetTarget(pos);
                        Debug.Log("Dash Attack state");
                        m_fHaloCharge = 0;
                        state = BossState.DashAttackState;

                    }
                    break;
                }
            case BossState.DashAttackState:
                {
                    if(m_fDashCharge < m_fDashChargeMax)
                    {
                        m_fDashCharge += Time.deltaTime;
                    }
                    else
                    {

                    }
                    break;
                }
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
        //Instantiate(Drop, transform.position, new Quaternion(0, 0, 0, 0));
        //Drop.GetComponent<Essence>().SetEnemyTag(transform.tag);
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

    private void resetSkillVariables()
    {
        Debug.Log("Idle state");
        state = BossState.Idle;

        //projectiles
        m_fConsecutiveShots = 0;
        m_fProjectileCooldown = 0;

        //slow attack
        Component bossHalo = GetComponent("Halo");
        bossHalo.GetType().GetProperty("enabled").SetValue(bossHalo, false, null);
        m_fHaloCharge = 0;
    }
}
