using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class OVRHandController : MonoBehaviour {
    public VRNode hand;
    public float animationSpeed; // Normalized speed.
    public GameObject handModelContainer;
    [HideInInspector]
    public float fistTarget, indexTarget, thumbTarget;
    [HideInInspector]
    public bool snapHand = true;
    private float currentFist = 0, currentIndex = 0, currentThumb = 0;
    private Animator handAnimator;
    private GameObject handModel;

	// Use this for initialization
	void Start () {
        handModel = handModelContainer.transform.GetChild(0).gameObject;
        handAnimator = handModel.GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void Update () {
        Vector3 handPosition = InputTracking.GetLocalPosition(hand);
        if (handPosition.sqrMagnitude != 0) { // If hands are being tracked, hand position != (0, 0, 0).
            transform.localPosition = InputTracking.GetLocalPosition(hand);
        }
        transform.localRotation = InputTracking.GetLocalRotation(hand);
        if (snapHand) {
            handModelContainer.transform.position = transform.position;
            handModelContainer.transform.rotation = transform.rotation;
        }
        moveToTarget(ref currentFist, fistTarget);
        moveToTarget(ref currentIndex, indexTarget);
        moveToTarget(ref currentThumb, thumbTarget);
        handAnimator.Play("Fist Curl", 0, currentFist);
        handAnimator.Play("Index Curl", 1, currentIndex);
        handAnimator.Play("Thumb Curl", 2, currentThumb);
        handAnimator.speed = 0;
    }

    void moveToTarget(ref float current, float target) {
        if (Mathf.Abs(target - current) > animationSpeed) {
            current += animationSpeed * (target - current) / Mathf.Abs(target - current);
        } else {
            current = target;
        }
    }
}
