using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour {

    public Transform Player;
    public float MinHeight = 4;
    public float FollowDist = 1.1f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float y = Player.position.y + FollowDist;
        if (y < MinHeight) y = MinHeight;
        transform.position = Vector3.Lerp(transform.position,new Vector3(Player.position.x, y, transform.position.z),0.4f);
	}
}
