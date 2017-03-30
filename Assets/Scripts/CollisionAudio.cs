using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionAudio : MonoBehaviour {
	public AudioSource tableHit;
	public float tableHitSqrThreshold;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnCollisionEnter(Collision collision) {
		print (collision.relativeVelocity.sqrMagnitude);
		if (collision.relativeVelocity.sqrMagnitude > 1) {
			tableHit.Play ();
		}
	}
}
