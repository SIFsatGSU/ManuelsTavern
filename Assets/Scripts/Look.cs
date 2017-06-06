using UnityEngine;
using UnityEngine.VR;
using System.Collections;
using System.IO;
//using UnityStandardAssets.ImageEffects;

public class Look : MonoBehaviour {
    public Camera playerCamera;
	public float mouseEnable; // 1 = enabled, 0 = disabled.
    public float xRotationSpeed;
    public float yRotationSpeed;
    public float yRotationAngle;

    [HideInInspector]
    public bool oculusControllerMode;
    private float currentYRotation = 0;
    // Use this for initialization
    void Start () {
		Cursor.visible = false;
	}
	
	// Update is called once per frame
	void Update () {
		// Rotate camera.
        float xRotation, yRotation;
        if (!oculusControllerMode) {
            xRotation = xRotationSpeed * Input.GetAxisRaw("Look X") * Time.deltaTime;
            yRotation = yRotationSpeed * Input.GetAxisRaw("Look Y") * Time.deltaTime;
            xRotation += mouseEnable * xRotationSpeed * Input.GetAxis("Mouse X") * Time.deltaTime;
            yRotation += mouseEnable * yRotationSpeed * Input.GetAxis("Mouse Y") * Time.deltaTime;
        } else {
            xRotation = xRotationSpeed * OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x * Time.deltaTime;
            yRotation = yRotationSpeed * OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y * Time.deltaTime;
        }
		if (GetComponent<RayCastController>().detailViewingMode) {
			currentYRotation = 0;
		} else {
			currentYRotation = Mathf.Clamp(currentYRotation + yRotation, -yRotationAngle / 2, yRotationAngle / 2);
		}
        transform.Rotate(new Vector3(0, 1, 0), xRotation);
        if (!VRDevice.isPresent) {
            // Only rotate Y when not in VR.
            playerCamera.transform.localEulerAngles = new Vector3(-currentYRotation, 0, 0);
        }
	}
}
