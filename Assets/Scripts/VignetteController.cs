using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public class VignetteController : MonoBehaviour {
    public PostProcessingProfile postProcessingProfile;
    public float vignetteSpeed = .2f;
    public bool moving = false, panning = false;
	
	// Update is called once per frame
	void Update () {
        // Also shrink field of vision when moving using the vignette.
        VignetteModel.Settings vignetteSettings = postProcessingProfile.vignette.settings;
        if (moving || panning) {
            vignetteSettings.opacity = Mathf.Lerp(vignetteSettings.opacity, 1, vignetteSpeed);
        } else {
            vignetteSettings.opacity = Mathf.Lerp(vignetteSettings.opacity, 0, vignetteSpeed);
        }
        postProcessingProfile.vignette.settings = vignetteSettings;
    }
}
