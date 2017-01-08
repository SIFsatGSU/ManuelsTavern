using UnityEngine;
using System.Collections;


public class Movement : MonoBehaviour {
    public CharacterController characterController;
    public GameObject cameraContainer;
    public float maxSpeed;
    public float jumpStrength;
    public float acceleration;
    public float normalHeight;
    public float crouchHeight;
    private float gravity = .3f;
    private float ySpeed = 0;
    public float currentSpeed = 0;
    public Vector3 currentDirection = new Vector3();
	private Vector3 originalPosition;

    // Use this for initialization
    void Start () {
		originalPosition = characterController.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        if (characterController.isGrounded) {
            ySpeed = 0;
        }
        if (characterController.velocity.y == 0) { // Hit ceiling.
            ySpeed = -.1f;
        }
        if (Input.GetButton("Jump") && characterController.isGrounded) {
            ySpeed = jumpStrength;
        }
        if (Input.GetButton("Crouch")) {
            characterController.height = crouchHeight;
        } else {
            characterController.height = normalHeight;
        }
        characterController.center = new Vector3(0, characterController.height / 2, 0);
        cameraContainer.transform.localPosition = new Vector3(0, characterController.height - .15f, 0);
        Vector3 zMovement = transform.forward * Input.GetAxisRaw("Vertical");
        Vector3 xMovement = transform.right * Input.GetAxisRaw("Horizontal");
        Vector3 yMovement = transform.up * ySpeed;
        if (characterController.velocity.magnitude == 0) {
            currentSpeed = 0;
        }
        if (zMovement.magnitude > 0 || xMovement.magnitude > 0) {
            currentDirection = zMovement + xMovement;
            currentDirection.Normalize();
            currentSpeed = Mathf.Clamp(currentSpeed + acceleration, 0, maxSpeed);
        } else {
            currentSpeed = Mathf.Clamp(currentSpeed - acceleration, 0, maxSpeed);
        }
        characterController.Move((yMovement + currentDirection * currentSpeed) * Time.deltaTime);
        ySpeed -= gravity;
	}

	void OnTriggerEnter(Collider other) {
		if (other.tag == "Death trigger") {
			characterController.transform.position = originalPosition;
			Debug.Log ("doot");
		}
	}
}
