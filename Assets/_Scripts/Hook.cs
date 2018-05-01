using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : MonoBehaviour {
    private float m_fMaxLen = 0;
    private Vector3 m_vDir = Vector3.zero;
    private Player m_player;
    private LineRenderer m_lineRenderer;
    private bool m_isHooked = false;
    
    public void Init(Vector3 dir, float len)
    {
        m_vDir = dir;
        m_fMaxLen = len;
        m_player = GameManager.GetInstance().Player.GetComponent<Player>();
        m_lineRenderer = transform.GetComponent<LineRenderer>();
        Debug.Log("Hook: "+m_vDir);
    }

    void Update()
    {
        m_lineRenderer.SetPosition(0, m_player.transform.position);
        m_lineRenderer.SetPosition(1, transform.position);
        if (m_vDir.magnitude > 0)
        {
            transform.position += m_vDir * Time.deltaTime;
            float len = (m_player.transform.position - transform.position).magnitude;
            if (len > m_fMaxLen)
            {
                Debug.Log("Destroyed hook because reached length");
                DestroySelf();
            }
            Debug.Log(transform.position);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggered " + other.gameObject.name);
        if (other.gameObject.layer == 9)
        {
            Debug.Log("Hooked "+other.gameObject.name);
            m_isHooked = true;
            m_player.SetHookCollisionPoint(transform.position);
            m_vDir = Vector3.zero;
        }
        else
        {
            if(other.gameObject.tag.Contains("Player") == false)
            {
                if (other.gameObject.layer != 8)
                {

                    Debug.Log("Destroyed hook because hit something");
                    DestroySelf();
                }
            }
            else 
            if(m_isHooked)
            {
                Debug.Log("Destroying hook because player reached destination");
                Invoke("DestroySelf", 1);
            }
        }
    }

    public void DestroySelf()
    {
        Debug.Log("Hook destroyed");
        m_player.HookDestroyed();
        Destroy(gameObject);
    }

}
