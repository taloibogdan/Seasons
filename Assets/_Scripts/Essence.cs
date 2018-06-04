using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Essence : MonoBehaviour {
    public int MinEssenceNormal = 3;
    public int MaxEssenceNormal = 10;
    
    public string EnemyTag;

    private bool isColliding = false;
    private Rigidbody m_rigidbody;

    private float m_floatingCDmax = 1;
    private float m_floatingCD = 0;
    private float m_floatingDir = 1;
    private float m_floatingSpeed = 0.5f;

    public void SetEnemyTag(string EnemyTag)
    {
        this.EnemyTag = EnemyTag;
    }

    private int GetReward()
    {
        int Reward = 5;

        Debug.Log("Essence dropped by " + EnemyTag);

        //this has to be modified
        if (EnemyTag.Equals("Enemy"))
        {
            Reward = new System.Random().Next(MinEssenceNormal, MaxEssenceNormal);
            Debug.Log("Reward set to " + Reward);
        }
        return Reward;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag.Equals("Player") && !isColliding)
        {
            isColliding = true;
            other.GetComponent<Player>().AddEssence(GetReward());
            Destroy(gameObject);
        }
    }

    void Start()
    {
        m_floatingCD = Random.value * m_floatingCDmax;
        m_rigidbody = transform.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if(IsGrounded())
        {
            m_rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }
        if(m_rigidbody.constraints==RigidbodyConstraints.FreezeAll)
        {
            if(m_floatingCD < 0)
            {
                m_floatingCD = m_floatingCDmax;
                m_floatingDir *= -1;
            }
            m_floatingCD -= Time.deltaTime;
            transform.position += new Vector3(0, m_floatingDir * m_floatingSpeed * Time.deltaTime, 0);
        }
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, transform.localScale.y + 0.2f);
    }
}
