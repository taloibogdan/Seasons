using System;
using System.Collections;
using UnityEngine;

public enum BossState
{
    Idle,
    BasicAttackState,
    JumpAttackState,
    QuickTeleportState,
    SlowAttackState,
    SpawnTrapsState,
    DashAttackState
}

public class Boss : MonoBehaviour {

    public float HP = 10;
    public float Speed = 2;
    public float AggroRange = 15;
    public bool IsFlying = false;
    public GameObject Drop;
    public GameObject Trap;

    private BossState state = BossState.Idle;
    private float m_fInvincibilityCooldownMax = 1;
    private float m_fInvincibilityCooldown = -1;
    private float m_fInvinciBlinkLong = 0.2f;
    private float m_fInvinciBlinkShort = 0.1f;

    private float m_fConsecutiveShots = 0;
    private float m_fConsecutiveShotsMax = 2;
    private float m_fProjectileCooldownMax = 2;
    private float m_fProjectileCooldown = -1;

    //private float m_fPeakJumpHeight = 10;
    //private Vector3 m_JumpPosition;
    //private float m_fJumpTime = 3;

    private float m_fTeleportConsecutiveShots = 0;
    private float m_fTeleportConsecutiveShotsMax = 3;
    private float m_fTeleportProjectileCooldownMax = 2;
    private float m_fTeleportProjectileCooldown = -1;
    private Vector3 positionBeforeTeleport = new Vector3(-100, -100, -100);
    private bool nextLeft = true;

    private float m_fHaloCharge = 0;
    private float m_fHaloChargeMax = 2;

    private float m_fTrapsSpwan = 0;
    private float m_fTrapsSpawnMax = 3;
    private float m_fTrapsSpawnCooldownMax = 1;
    private float m_fTrapsSpawnCooldown = -1;

    private bool isCoroutineActive = false;

    private Renderer m_renderer;
    private Rigidbody m_rigidbody;
    private GameObject m_player;
    private GameManager m_gameManager;
    private ResourceManager m_resourceManager;

    // Use this for initialization
    void Start () {
        m_renderer = transform.GetComponentInChildren<SkinnedMeshRenderer>();
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

        if(dx < 0)
        {
            Quaternion rot = transform.rotation;
            rot.y = 0;
            transform.rotation = rot;
        }
        if (dx > 0)
        {
            Quaternion rot = transform.rotation;
            rot.y = 180;
            transform.rotation = rot;
        }

        float sgn = dx / Mathf.Abs(dx);
        if (Mathf.Abs(dx) > AggroRange)
        {
            if(positionBeforeTeleport != new Vector3(-100, -100, -100))
            {
                Vector3 position = transform.position;
                position.y = positionBeforeTeleport.y;
                transform.position = position;
            }
            resetSkillVariables();
            return;
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
                            proj.Speed = 7;
                            m_fConsecutiveShots++;
                        }
                        return;
                    }
                    else
                    {
                        if (!isCoroutineActive)
                            StartCoroutine(changeState(0.5f, BossState.QuickTeleportState));
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
            case BossState.QuickTeleportState:
                {
                    if (m_fTeleportConsecutiveShots < m_fTeleportConsecutiveShotsMax)
                    {
                        if (m_fTeleportProjectileCooldown > 0)
                        {
                            m_fTeleportProjectileCooldown -= Time.deltaTime;
                        }
                        else
                        {
                            m_fTeleportProjectileCooldown = m_fTeleportProjectileCooldownMax;

                            Vector3 newPosition = m_player.transform.position;
                            int height = new System.Random().Next(5, 8);
                            int direction = nextLeft ? -1 : 1;

                            newPosition.x += direction * 3;
                            newPosition.y = height;

                            Vector3 diff = m_resourceManager.WallRight.transform.position - newPosition;
                            if (diff.x <= 3)
                            {
                                newPosition.x -= 4;
                            }

                            transform.position = newPosition;

                            Projectile proj = Instantiate(m_resourceManager.EnemyProjectile, transform.position, Quaternion.Euler(0, 0, 0)).GetComponent<Projectile>();
                            proj.SetLifetime(4);
                            proj.SetTarget(m_player.transform.position);
                            m_fTeleportConsecutiveShots++;
                            nextLeft = !nextLeft;
                        }
                        return;
                    }
                    else
                    {
                        if(!isCoroutineActive)
                            StartCoroutine(changeState(1, BossState.SlowAttackState));
                        
                    }
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
                        if (!isCoroutineActive)
                            StartCoroutine(changeState(1, BossState.SpawnTrapsState));

                    }
                    break;
                }
            case BossState.SpawnTrapsState:
                {
                    if (m_fTrapsSpwan < m_fTrapsSpawnMax)
                    {
                        if (m_fTrapsSpawnCooldown > 0)
                        {
                            m_fTrapsSpawnCooldown -= Time.deltaTime;
                        }
                        else
                        {
                            m_fTrapsSpawnCooldown = m_fTrapsSpawnCooldownMax;

                            Vector3 newPosition = m_player.transform.position;

                            double posDiff = new System.Random().NextDouble() * 5;

                            newPosition.x += pos.x - 3 + + Convert.ToSingle(posDiff); ;
                            newPosition.y = 8;

                            var trap = Instantiate(Trap, newPosition, Quaternion.Euler(180, 0, 0));
                            trap.GetComponent<Rigidbody>().isKinematic = false;
                            trap.GetComponent<Rigidbody>().useGravity = true;
                            trap.gameObject.tag = "BossTrap";
                            Destroy(trap, Time.time + 5);
                            m_fTrapsSpwan++;
                        }
                    }
                    else
                    {
                        if (!isCoroutineActive)
                            StartCoroutine(changeState(1, BossState.BasicAttackState));
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
        System.Random random = new System.Random();

        for (int i = 0; i < 5; i++)
        {
            double posDiff = random.NextDouble() * 6;
            Vector3 bossPosition = transform.position;
            bossPosition.x += (-3 + Convert.ToSingle(posDiff));
            Instantiate(Drop, bossPosition, new Quaternion(0, 0, 0, 0));
            Drop.GetComponent<Essence>().SetEnemyTag(transform.tag);
        }

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
        state = BossState.Idle;

        //projectiles
        m_fConsecutiveShots = 0;
        m_fProjectileCooldown = 0;
        m_fTeleportConsecutiveShots = 0;
        m_fTeleportProjectileCooldown = 0;
        m_fTrapsSpwan = 0;
        m_fTrapsSpawnCooldown = 0;

        //slow attack
        Component bossHalo = GetComponent("Halo");
        bossHalo.GetType().GetProperty("enabled").SetValue(bossHalo, false, null);
        m_fHaloCharge = 0;
    }

    private IEnumerator changeState(float seconds, BossState nextState)
    {
        isCoroutineActive = true;

        yield return new WaitForSeconds(seconds);

        switch(nextState)
        {
            case BossState.QuickTeleportState:
                {
                    m_fConsecutiveShots = 0;
                    m_fProjectileCooldown = 0;
                    //Debug.Log("Jump Attack state");
                    //state = BossState.JumpAttackState;
                    //m_JumpPosition = m_player.transform.position;
                    //m_rigidbody.useGravity = false;
                    Debug.Log("Quick Teleport state");
                    positionBeforeTeleport = transform.position;
                    m_rigidbody.useGravity = false;
                    break;
                }
            case BossState.SlowAttackState:
                {
                    m_fTeleportConsecutiveShots = 0;
                    m_fTeleportProjectileCooldown = 0;
                    int direction = (new System.Random().Next() % 10 < 5) ? -1 : 1;
                    Vector3 newPosition = m_player.transform.position;

                    //check if there is a wall at the new position (newPosition.x + 3 * direction)
                    if (m_resourceManager.WallRight.transform.position.x - newPosition.x <= 5)
                    {
                        newPosition.x = m_player.transform.position.x - 5;
                    }
                    else
                    {
                        newPosition.x += 5 * direction;
                    }
                    
                    newPosition.y = positionBeforeTeleport.y;
                    transform.position = newPosition;

                    Debug.Log("Slow Attack state");
                    m_rigidbody.useGravity = true;
                    Component bossHalo = GetComponent("Halo");
                    bossHalo.GetType().GetProperty("enabled").SetValue(bossHalo, true, null);
                    break;
                }
            case BossState.SpawnTrapsState:
                {
                    Component bossHalo = GetComponent("Halo");
                    bossHalo.GetType().GetProperty("enabled").SetValue(bossHalo, false, null);
                    Projectile proj = Instantiate(m_resourceManager.SlowProjectile, transform.position, Quaternion.Euler(0, 0, 0)).GetComponent<Projectile>();
                    proj.SetTarget(m_player.transform.position);
                    Debug.Log("Spawn Traps state");
                    m_fHaloCharge = 0;
                    break;
                }
            case BossState.BasicAttackState:
                {
                    m_fTrapsSpwan = 0;
                    m_fTrapsSpawnCooldown = 0;
                    break;
                }
        }
        state = nextState;
        isCoroutineActive = false;
    }
}
