using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionAudio : MonoBehaviour {
	public AudioSource audioSource;
    public float minHitVelocity;
    public float maxHitVelocity;
    public string[] collisionObjectTags;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnCollisionEnter(Collision collision) {
        bool tagAccepted = false;
        foreach (string tag in collisionObjectTags) {
            if (collision.gameObject.tag == tag) tagAccepted = true;
        }
        if (!tagAccepted) return;
        float velocity = collision.relativeVelocity.magnitude;
		print (velocity);
        if (velocity > minHitVelocity) {
            float volume = (velocity - minHitVelocity) / (maxHitVelocity - minHitVelocity);
            audioSource.volume = Mathf.Clamp01(volume);
            audioSource.Play();
		}
	}
}
