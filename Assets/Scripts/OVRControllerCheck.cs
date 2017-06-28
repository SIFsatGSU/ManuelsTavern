using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OVRControllerCheck : MonoBehaviour {
    private bool set = false;
    public GameObject clipboardContainer;
    public int numberOfFramesTested;
	public TextMesh text;

    // Use this for initialization
	void Update () {
        if (numberOfFramesTested > 0) {
            if (OVRInput.IsControllerConnected(OVRInput.Controller.LTouch) &&
                    OVRInput.IsControllerConnected(OVRInput.Controller.RTouch) && !set) {
				GameManager.oculusControllerMode = true;
                GetComponent<Look>().oculusControllerMode = true;
                GetComponent<RayCastController>().oculusControllerMode = true;
                GetComponentInChildren<ClipboardController>().oculusControllerMode = true;
                clipboardContainer.transform.parent = null;
				text.text = "Pull trigger to\nview details";
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
