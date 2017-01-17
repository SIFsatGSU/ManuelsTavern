using UnityEngine;
using System.Collections;


public class Movement : MonoBehaviour {
    public CharacterController characterController;
    public GameObject cameraContainer;
    public float maxSpeed;
    public float jumpStrength;
    public float normalHeight;
    public float crouchHeight;
    private float gravity = .3f;
	private float ySpeed = 0;
	public Vector3 forwardVector;
	private Vector3 originalPosition;

    // Use this for initialization
    void Start () {
		originalPosition = characterController.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        if (characterController.velocity.y == 0) { // Hit ceiling.
            ySpeed = -.11f;
        }
		if (characterController.isGrounded) {
			ySpeed = 0;
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
		Vector3 rightVector = Vector3.Cross (transform.up, forwardVector);
		Vector3 zMovement = forwardVector * Input.GetAxisRaw("Vertical");
		Vector3 xMovement = rightVector * Input.GetAxisRaw("Horizontal");
        Vector3 yMovement = transform.up * ySpeed;
		characterController.Move((yMovement + zMovement + xMovement) * Time.deltaTime);
        ySpeed -= gravity;
	}

	void OnTriggerEnter(Collider other) {
		if (other.tag == "Death trigger") {
			characterController.transform.position = originalPosition;
			Debug.Log ("doot");
		}
	}
}
