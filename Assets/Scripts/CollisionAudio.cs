using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionAudio : MonoBehaviour {
	public AudioSource audioSource;
    public float minHitVelocity;
    public float maxHitVelocity;
    public string[] collisionObjectTags;

	void OnCollisionEnter(Collision collision) {
        bool tagAccepted = false;
        foreach (string tag in collisionObjectTags) {
            if (collision.gameObject.tag == tag) tagAccepted = true;
        }
        if (!tagAccepted) return;
        float velocity = collision.relativeVelocity.magnitude;
        if (velocity > minHitVelocity) {
            float volume = (velocity - minHitVelocity) / (maxHitVelocity - minHitVelocity);
			print (volume);
            audioSource.volume = Mathf.Clamp01(volume);
            audioSource.Play();
		}
	}
}
