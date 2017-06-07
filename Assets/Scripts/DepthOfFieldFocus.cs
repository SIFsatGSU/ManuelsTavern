using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public class DepthOfFieldFocus : MonoBehaviour {
	public PostProcessingProfile profile;
	public Transform focusTransform;

	// Update is called once per frame
	void Update () {
		DepthOfFieldModel.Settings settings = profile.depthOfField.settings;
		settings.focusDistance = 
			Vector3.Dot (transform.forward, focusTransform.position - transform.position);
		profile.depthOfField.settings = settings;
	}
}
