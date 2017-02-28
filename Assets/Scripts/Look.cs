using UnityEngine;
using UnityEngine.VR;
using System.Collections;
using System.IO;
using UnityStandardAssets.ImageEffects;

public class Look : MonoBehaviour {
    public Camera playerCamera;
    public GameObject UIElement;
	public GameObject reticle;
	public GameObject clickSign;
	public GameObject clipboard;

	public Material pageMaterialPrefab;
	public Animator clipboardAnimator;

	public float mouseEnable; // 1 = enabled, 0 = disabled.
    public float xRotationSpeed;
    public float yRotationSpeed;
    public float yRotationAngle;
    public float UIElementSize;
	public float clickSignAlphaSpeed;
    private float currentYRotation = 0;
	private float clickSignAlpha = 0;
	private bool detailViewingMode = false;
	private string pictureLookedAt = "";
	private string wallLookedAt = "";
	private Hashtable pictureFolderMap = new Hashtable();
	private bool scrollable = true;
	private ClipboardController clipboardController;

    // Use this for initialization
    void Start () {
		pictureFolderMap ["Pictures Main Room Left"] = "Main Room Left";
		Cursor.visible = false;
		clipboardController = clipboard.GetComponent<ClipboardController> ();
	}
	
	// Update is called once per frame
	void Update () {
		// Rotate camera.
		float xRotation = xRotationSpeed * Input.GetAxisRaw("Look X") * Time.deltaTime;
		float yRotation = yRotationSpeed * Input.GetAxisRaw("Look Y") * Time.deltaTime;
		xRotation += mouseEnable * xRotationSpeed * Input.GetAxis("Mouse X") * Time.deltaTime;
		yRotation += mouseEnable * yRotationSpeed * Input.GetAxis("Mouse Y") * Time.deltaTime;
		if (detailViewingMode) {
			currentYRotation = 0;
		} else {
			currentYRotation = Mathf.Clamp(currentYRotation + yRotation, -yRotationAngle / 2, yRotationAngle / 2);
		}
        transform.Rotate(new Vector3(0, 1, 0), xRotation);
        playerCamera.transform.localEulerAngles = new Vector3(-currentYRotation, 0, 0);

        // Ray cast
        Ray forwardRay;
        if (VRDevice.isPresent) {
            Vector3 forward = InputTracking.GetLocalRotation(VRNode.Head) * new Vector3(0, 0, 1);
            forward = transform.rotation * forward;
            forwardRay = new Ray(playerCamera.transform.position, forward);
        } else {
            forwardRay = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        }

		GetComponent<Movement>().forwardVector = new Vector3(forwardRay.direction.x, 0, forwardRay.direction.z).normalized;

        Debug.DrawRay(forwardRay.origin, forwardRay.direction);
        RaycastHit hit;
		bool showingClickSign = false;
		pictureLookedAt = "";
        if (Physics.Raycast(forwardRay, out hit)) {
            Vector3 hitProjection = hit.point - playerCamera.transform.position;
            float distance = hitProjection.magnitude;
			UIElement.transform.localPosition = new Vector3(0, 0, distance);
			float reticleScale = distance * UIElementSize;
			UIElement.transform.localScale = new Vector3(reticleScale, reticleScale, reticleScale);
            if (hit.transform.gameObject.tag == "PictureFrame") {
				showingClickSign = true;
                GameObject pictureFrame = hit.transform.gameObject;
				pictureLookedAt = hit.transform.name;
				wallLookedAt = hit.transform.parent.name;
            }
        }

		// To animate the alpha channel of the "Click to view detail" sign.
		if (showingClickSign) {
			clickSignAlpha = Mathf.Min(clickSignAlpha + clickSignAlphaSpeed, 1);
		} else {
			clickSignAlpha = Mathf.Max(clickSignAlpha - clickSignAlphaSpeed, 0);
		}
		Color reticleColor = reticle.GetComponent<Renderer> ().material.color;
		reticle.GetComponent<Renderer> ().material.color = new Color(reticleColor.r, reticleColor.g, reticleColor.b, clickSignAlpha);
		clickSign.GetComponent<TextMesh> ().color = new Color(1,1,1, clickSignAlpha);

		// Start detail viewing mode.
		if (!detailViewingMode && Input.GetAxisRaw("Use") > 0 && pictureLookedAt != "") {
			detailViewingMode = true;
			reticle.GetComponent<MeshRenderer> ().enabled = false;
			clickSign.GetComponent<MeshRenderer> ().enabled = false;
			playerCamera.transform.localEulerAngles = new Vector3(0, 0, 0);
			//clipboardContainer.transform.LookAt (playerCamera.transform.position);

			// Initiate the detail pages materials.
			string path = "Picture frames/" + pictureFolderMap[wallLookedAt] + "/" + pictureLookedAt + "/" + "Details";
			Object[] textures = Resources.LoadAll (path);
			clipboardController.detailPages = new Material[textures.Length];
			if (textures.Length > 0) {
				for (int i = 0; i < textures.Length; i++) {
					clipboardController.detailPages [i] = Instantiate (pageMaterialPrefab);
					clipboardController.detailPages [i].mainTexture = (Texture2D)textures [i];
				}
				// Page 1's detail material is the second material.
				clipboardController.currentViewingPage = 0;
			}
			clipboard.GetComponent<ClipboardController> ().Show ();
			GetComponentInChildren<DepthOfField> ().enabled = true;
		}

		// End detail viewing mode.
		if (detailViewingMode && Input.GetAxisRaw("Cancel") > 0) {
			detailViewingMode = false;
			clipboardController.Hide ();
			reticle.GetComponent<MeshRenderer> ().enabled = true;
			clickSign.GetComponent<MeshRenderer> ().enabled = true;
			GetComponentInChildren<DepthOfField> ().enabled = false;
		}

		// Refresh scrollability
		if (!scrollable && clipboardAnimator.GetCurrentAnimatorStateInfo (0).normalizedTime >= 1) {
			scrollable = true;
		}

		// Handle the detail page flipping.
		if (detailViewingMode && clipboardController.detailPages.Length > 0 && scrollable) {
			float scroll = Input.GetAxisRaw ("Details Scroll");
			if (scroll < 0) { // Scroll down
				scrollable = false;
				clipboardController.FlipForward ();
			} else if (scroll > 0) { // Scroll up
				scrollable = false;
				clipboardController.FlipBackward ();
			}
		}
	}

}
