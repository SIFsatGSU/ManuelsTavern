using UnityEngine;
using UnityEngine.VR;
using System.Collections;

public class Look : MonoBehaviour {
    public Camera playerCamera;
    public GameObject reticle;

    public float mouseEnable; // 1 = enabled, 0 = disabled.
    public float xRotationSpeed;
    public float yRotationSpeed;
    public float yRotationAngle;
    public float reticleSize;
    private float currentYRotation = 0;
    // Use this for initialization
    void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        float xRotation = xRotationSpeed * Input.GetAxisRaw("Look X") * Time.deltaTime;
        float yRotation = yRotationSpeed * Input.GetAxisRaw("Look Y") * Time.deltaTime;
        xRotation += mouseEnable * xRotationSpeed * Input.GetAxisRaw("Mouse X") * Time.deltaTime;
        yRotation += mouseEnable * yRotationSpeed * Input.GetAxisRaw("Mouse Y") * Time.deltaTime;
        currentYRotation = Mathf.Clamp(currentYRotation + yRotation, -yRotationAngle / 2, yRotationAngle / 2);

        transform.Rotate(new Vector3(0, 1, 0), xRotation);
        playerCamera.transform.localEulerAngles = new Vector3(-currentYRotation, 0, 0);

        // Ray cast
        GameObject[] pictureFrames = GameObject.FindGameObjectsWithTag("PictureFrame");
        foreach (GameObject frame in pictureFrames) {
            frame.transform.GetChild(0).GetComponent<Renderer>().enabled = false;
        }

        Ray forwardRay;
        if (VRDevice.isPresent) {
            Vector3 forward = InputTracking.GetLocalRotation(VRNode.Head) * new Vector3(0, 0, 1);
            forward = transform.rotation * forward;
            forwardRay = new Ray(playerCamera.transform.position, forward);
        } else {
            forwardRay = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        }
        Debug.DrawRay(forwardRay.origin, forwardRay.direction);
        RaycastHit hit;
        if (Physics.Raycast(forwardRay, out hit)) {
            Vector3 hitProjection = hit.point - playerCamera.transform.position;
            float distance = hitProjection.magnitude;
            reticle.transform.localPosition = new Vector3(0, 0, distance);
            float reticleScale = distance * reticleSize;
            reticle.transform.localScale = new Vector3(reticleScale, reticleScale, reticleScale);
            if (hit.transform.gameObject.tag == "PictureFrame") {
                GameObject pictureFrame = hit.transform.gameObject;
                GameObject pictureText = pictureFrame.transform.GetChild(0).gameObject;
                pictureText.GetComponent<Renderer>().enabled = true;
            }
        }
	}
}
