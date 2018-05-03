using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public GameObject Trap;
    private bool m_isPressed = false;
    void Start()
    {
        Trap.GetComponent<Rigidbody>().isKinematic = true;
        Trap.GetComponent<Rigidbody>().useGravity = false;
    }

    void Update()
    {
        if (Trap != null && Trap.transform.position.z < -100)
        {
            Destroy(Trap);
            Trap = null;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag.Contains("Player") && m_isPressed == false)
        {
            Trap.GetComponent<Rigidbody>().isKinematic = false;
            Trap.GetComponent<Rigidbody>().useGravity = true;
            m_isPressed = true;
            transform.position -= new Vector3(0, .1f, 0);
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.tag.Contains("Player") && m_isPressed)
        {
            m_isPressed = false;
            transform.position += new Vector3(0, .1f, 0);
        }
    }
}
