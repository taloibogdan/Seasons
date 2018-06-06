using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour {

    public Transform Player;
    public float MinHeight = 4;
    public float FollowDist = 1.1f;
    public GameObject WallLeft;

    private bool isInArena = false;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        float y = Player.position.y + FollowDist;
        if (y < MinHeight) y = MinHeight;

        if (transform.position.x >= WallLeft.transform.position.x + 12)
        {
            isInArena = true;
            Debug.Log("Player is in the arena");
        }

        if(isInArena)
        {
            WallLeft.GetComponent<BoxCollider>().enabled = true;
            WallLeft.GetComponent<MeshRenderer>().enabled = true;
        }
        
        transform.position = Vector3.Lerp(transform.position,new Vector3(Player.position.x, y, transform.position.z),0.4f);
	}
}
