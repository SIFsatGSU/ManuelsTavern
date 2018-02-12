using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRControllerCheck : MonoBehaviour {
    private bool set = false;
    public GameObject clipboardContainer;
    public int numberOfFramesTested;
	public TextMesh text;
    [HideInInspector]
    public bool vrMode = false;

    // Use this for initialization
	void Update () {
        if (numberOfFramesTested > 0) {
            numberOfFramesTested--;
            if (OVRInput.IsControllerConnected(OVRInput.Controller.LTouch) &&
                    OVRInput.IsControllerConnected(OVRInput.Controller.RTouch) && !set) {
				GameManager.oculusControllerMode = true;
                vrMode = true;
                clipboardContainer.transform.parent = null;
				text.text = "Pull trigger to\nview details";
                set = true;
                numberOfFramesTested = 0;
                clipboardContainer.GetComponent<GrabMotionTrack>().enabled = true;
                clipboardContainer.AddComponent<Rigidbody>()    ;
            }
        }
    }
}
