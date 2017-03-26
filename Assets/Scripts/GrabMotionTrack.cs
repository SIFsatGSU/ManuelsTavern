using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabMotionTrack : MonoBehaviour {
    [HideInInspector]
    public bool enable;
    public float velocityAlpha;
    public float angularVelocityAlpha;
    private bool enableToggle;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private Vector3 currentVelocity;
    private Vector3 currentAngularVelocity;
    private Rigidbody rigidBody;

	// Use this for initialization
	void Start () {
        rigidBody = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
        if (!enableToggle && enable) { // From disabled to enable.
            lastPosition = transform.position;
            lastRotation = transform.rotation;
            rigidBody.velocity = new Vector3(0, 0, 0);
            rigidBody.angularVelocity = new Vector3(0, 0, 0);
            GetComponent<Collider>().enabled = false;
			//rigidBody.useGravity = false;
			//rigidBody.isKinematic = true;
        } else if (enableToggle && enable) {
            Vector3 targetVelocity = (transform.localPosition - lastPosition) / Time.deltaTime;
            Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(lastRotation);
            Vector3 deltaEuler = deltaRotation.eulerAngles;
            Vector3 targetAngularVelocity = new Vector3(Mathf.DeltaAngle(0, deltaEuler.x), Mathf.DeltaAngle(0, deltaEuler.y), Mathf.DeltaAngle(0, deltaEuler.z))
                    * Mathf.PI / 180 / Time.deltaTime;
            currentVelocity = (1 - velocityAlpha) * rigidBody.velocity + velocityAlpha * targetVelocity;
            currentAngularVelocity = (1 - angularVelocityAlpha) * rigidBody.angularVelocity + angularVelocityAlpha * targetAngularVelocity;
            rigidBody.velocity = currentVelocity;
            rigidBody.angularVelocity = currentAngularVelocity;
            lastPosition = transform.position;
            lastRotation = transform.rotation;
        } else if (enableToggle && !enable) {
			//rigidBody.useGravity = true;
			//rigidBody.isKinematic = false;
            GetComponent<Collider>().enabled = true;
        }
        enableToggle = enable;
	}
}
