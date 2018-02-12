using UnityEngine;
using UnityEngine.VR;
using System.Collections;
using UnityEngine.PostProcessing;

public class Movement : MonoBehaviour {
	public GameObject cameraContainer;
    public Camera playerCamera;
    public float speed;
    public float jumpStrength;
    public float normalHeight;
    public float crouchHeight;
	public float ySpeed = 0;
	private CharacterController characterController;
	private float gravity = 10.3f;
    private Vector3 originalPosition;
    private Vector2 headPosition;
    private VignetteController vignetteController;

    // Use this for initialization
    void Start () {
		characterController = GetComponent<CharacterController> ();
		originalPosition = characterController.transform.position;
		// Ignore collision with clipboard.
		Physics.IgnoreCollision (GameObject.FindGameObjectWithTag ("Clipboard").GetComponent<Collider> (),
			characterController.GetComponent<Collider> ());
        vignetteController = GetComponent<VignetteController>();
    }
	
	// Update is called once per frame
	void Update () {
        if (characterController.velocity.y == 0) { // Hit ceiling.
            //ySpeed = -.22f;
        }
		if (characterController.isGrounded) {
			ySpeed = 0;
		}
		if (!GameManager.oculusControllerMode) {
            if (Input.GetButton("Jump") && characterController.isGrounded) {
                ySpeed = jumpStrength;
            }
            /*if (Input.GetButton("Crouch")) {
                characterController.height = crouchHeight;
            } else {
                characterController.height = normalHeight;
            }*/
        } else { // Specific handler for Oculus Rift controller.

        }

        characterController.center = new Vector3(0, characterController.height / 2, 0);
        /*cameraContainer.transform.localPosition = new Vector3(cameraContainer.transform.localPosition.x,
                characterController.height - .15f, cameraContainer.transform.localPosition.z);*/
        Vector3 zMovement, xMovement, yMovement;
        Vector3 forwardVector = playerCamera.transform.forward;
        forwardVector[1] = 0;
        forwardVector.Normalize();
        Vector3 rightVector = Vector3.Cross(transform.up, forwardVector);
		if (!GameManager.oculusControllerMode) {
            zMovement = forwardVector * Input.GetAxisRaw("Vertical");
            xMovement = rightVector * Input.GetAxisRaw("Horizontal");
        } else { // Specific handler for Oculus Rift controller.
            zMovement = forwardVector * OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;
            xMovement = rightVector * OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x;
        }
        yMovement = transform.up * ySpeed;
        Vector3 finalMovement = (yMovement + (zMovement + xMovement).normalized * speed);
        characterController.Move(finalMovement * Time.deltaTime);
        ySpeed -= gravity * Time.deltaTime;

        if (UnityEngine.XR.XRDevice.isPresent) { // Use head movement to move player to always center the camera.
            Vector3 currentHeadPosition = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.Head);
            float deltaZ = currentHeadPosition.z - headPosition.y;
            float deltaX = currentHeadPosition.x - headPosition.x;

            cameraContainer.transform.localPosition = new Vector3(cameraContainer.transform.localPosition.x - deltaX,
                    cameraContainer.transform.localPosition.y, cameraContainer.transform.localPosition.z - deltaZ);
            characterController.Move(transform.forward * deltaZ + transform.right * deltaX);
            headPosition.x = currentHeadPosition.x;
            headPosition.y = currentHeadPosition.z;

            vignetteController.moving = finalMovement.x != 0 || finalMovement.z != 0;
        }
    }

	void OnTriggerEnter(Collider other) {
		if (other.tag == "Death trigger") {
			characterController.transform.position = originalPosition;
		}
	}

	void OnCollisionEnter(Collision col) {
		if (col.gameObject.tag == "Clipboard") {
			print ("AHHHHHHH");
		}
	}
}
