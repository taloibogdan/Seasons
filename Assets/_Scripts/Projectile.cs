using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    public bool IsPlayerProjectile = true;
    public float Speed = 8;
    public bool IsFollowingPlayer = false;
    public float Lifetime = 2;

    private Vector3 m_v3TargetPos;
    private GameObject m_player = null;
    private bool m_isStarted = false;
    private Vector3 m_v3Speed;

    public void SetTarget(Vector3 pos)
    {
        m_v3TargetPos = pos + (pos - transform.position).normalized * Speed * Lifetime;
        m_isStarted = true;
    }

    void Start()
    {
        if (IsFollowingPlayer)
        {
            m_player = GameManager.GetInstance().Player;
            m_isStarted = true;
        }
        if(IsFollowingPlayer)
        {
            m_v3TargetPos = m_player.transform.position;
        }
        m_v3Speed = m_v3TargetPos - transform.position;
        m_v3Speed = m_v3Speed.normalized * Speed;
    }

    // Update is called once per frame
    void Update () {
        Lifetime -= Time.deltaTime;
        if (Lifetime < 0) Destroy(gameObject);
		if(IsFollowingPlayer)
        {
            m_v3TargetPos = m_player.transform.position;
        }
        Vector3 newSpeed = m_v3TargetPos - transform.position;
        newSpeed = newSpeed.normalized * Speed;
        m_v3Speed = Vector3.Lerp(m_v3Speed, newSpeed, .5f);
        transform.position += (m_v3Speed * Time.deltaTime);
	}
    void OnTriggerEnter(Collider c)
    {
        //Debug.Log(c.transform.tag);
        if (IsPlayerProjectile && c.transform.tag == "Enemy")
        {
            c.transform.GetComponent<Enemy>().GetDamaged();
            Destroy(gameObject);
        }
        if (IsPlayerProjectile == false && c.transform.tag == "PlayerCollider")
        {
            c.transform.parent.GetComponent<Player>().GetDamaged();
            Destroy(gameObject);
        }
        if(c.transform.tag != "Enemy" && !c.transform.tag.Contains("Player") && c.gameObject.layer != 8)
        {
            Destroy(gameObject);
        }
    }
}
