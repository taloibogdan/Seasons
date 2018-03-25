﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {
    public float HP = 2;
    public float Speed = 2;
    public float Range = 3;
    public bool IsFlying = false;
    public bool IsRanged = false;

    private float m_fInvincibilityCooldownMax = 1;
    private float m_fInvincibilityCooldown = -1;
    private float m_fInvinciBlinkLong = 0.2f;
    private float m_fInvinciBlinkShort = 0.1f;

    private Renderer m_renderer;
    private GameObject m_player;
    private GameManager m_gameManager;
	void Start ()
    {
        m_renderer = transform.GetComponent<Renderer>();
        m_gameManager = GameManager.GetInstance();
        m_player = m_gameManager.Player;
	}
	
	// Update is called once per frame
	void Update () {
        if(m_gameManager.GameRunning == false)
        {
            return;
        }
        if (IsFlying == false)
        {
            Vector3 pos = m_player.transform.position;
            float dx = pos.x - transform.position.x;
            float sgn = dx / Mathf.Abs(dx);
            if (dx * sgn > Speed * Time.deltaTime && Range < (pos - transform.position).magnitude)
            {
                transform.position += new Vector3(Speed * Time.deltaTime * sgn, 0, 0);
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
        Destroy(gameObject);
    }
    void OnTriggerStay(Collider c)
    {
        if(c.transform.tag.Equals("Player"))
        {
            c.transform.parent.GetComponent<Player>().GetDamaged();
        }
    }
}
