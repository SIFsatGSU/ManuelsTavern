using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OVRControllerCheck : MonoBehaviour {
    private bool set = false;
    public GameObject clipboardContainer;
    public int numberOfFramesTested;
    // Use this for initialization
	void Update () {
        if (numberOfFramesTested > 0) {
            if (OVRInput.IsControllerConnected(OVRInput.Controller.LTouch) &&
                    OVRInput.IsControllerConnected(OVRInput.Controller.RTouch) && !set) {
                GetComponent<Movement>().oculusControllerMode = true;
                GetComponent<Look>().oculusControllerMode = true;
                GetComponent<RayCastController>().oculusControllerMode = true;
                GetComponentInChildren<ClipboardController>().oculusControllerMode = true;
                clipboardContainer.transform.parent = null;
                set = true;
                numberOfFramesTested = 0;
            } else {
                clipboardContainer.GetComponent<GrabMotionTrack>().enabled = false;
                Destroy(clipboardContainer.GetComponent<Rigidbody>());
                numberOfFramesTested--;
            }
        }
	}
}
