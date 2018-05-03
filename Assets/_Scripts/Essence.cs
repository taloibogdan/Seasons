using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Essence : MonoBehaviour {
    public int MinEssenceNormal = 3;
    public int MaxEssenceNormal = 10;
    
    public string EnemyTag;

    private bool isColliding = false;
    private Rigidbody m_rigidbody;

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
        m_rigidbody = transform.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if(IsGrounded())
        {
            m_rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, transform.localScale.y + 0.2f);
    }
}
