using UnityEngine;
using UnityEngine.VR;
using System.Collections;
using System.IO;

public class Look : MonoBehaviour {
    public Camera playerCamera;
    public GameObject UIElement;
	public GameObject reticle;
	public GameObject clickSign;
	public GameObject clipboard;
	public Animator clipboardAnimator;
	public Animator clipboardShowHideAnimator;
	public GameObject page1;
	public GameObject page2;
	public GameObject page3;
	public Material paperMaterial;
	public Material pageMaterialPrefab;

	public float mouseEnable; // 1 = enabled, 0 = disabled.
    public float xRotationSpeed;
    public float yRotationSpeed;
    public float yRotationAngle;
    public float UIElementSize;
	public float clickSignAlphaSpeed;
    private float currentYRotation = 0;
	private float clickSignAlpha = 0;
	private bool detailViewingMode = false;
	GameObject[] pictureFrames;
	private string pictureLookedAt = "";
	private string wallLookedAt = "";
	private int currentViewingPage = 0;
	private Hashtable pictureFolderMap = new Hashtable();
	private bool clipboardReflectionRefresh;
	private Material[] detailPages = new Material[0];
	private bool scrollable = true;

    // Use this for initialization
    void Start () {
		pictureFrames = GameObject.FindGameObjectsWithTag("PictureFrame");
		pictureFolderMap ["Pictures Main Room Left"] = "Main Room Left";
		clipboardAnimator.Play ("Page 1 flip reversed", 0, 1);
		clipboardShowHideAnimator.Play ("Hide Clipboard", 0, 1);
	}
	
	// Update is called once per frame
	void Update () {
		// Rotate camera.
		float xRotation = xRotationSpeed * Input.GetAxisRaw("Look X") * Time.deltaTime;
        float yRotation = yRotationSpeed * Input.GetAxisRaw("Look Y") * Time.deltaTime;
        xRotation += mouseEnable * xRotationSpeed * Input.GetAxisRaw("Mouse X") * Time.deltaTime;
		yRotation += mouseEnable * yRotationSpeed * Input.GetAxisRaw("Mouse Y") * Time.deltaTime;
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
		clickSign.GetComponent<TextMesh> ().color = new Color(1,1,1, clickSignAlpha);

		// Start detail viewing mode.
		if (!detailViewingMode && Input.GetAxisRaw("Use") > 0 && pictureLookedAt != "") {
			detailViewingMode = true;
			clipboardShowHideAnimator.Play ("Show Clipboard");
			clipboardAnimator.Play ("Page 1 flip reversed", 0, 1);
			reticle.GetComponent<MeshRenderer> ().enabled = false;
			clickSign.GetComponent<MeshRenderer> ().enabled = false;
			clipboardReflectionRefresh = true;

			// Initiate the detail pages materials.
			string path = pictureFolderMap[wallLookedAt] + "/" + pictureLookedAt;
			Object[] textures = Resources.LoadAll (path);
			detailPages = new Material[textures.Length];
			if (textures.Length > 0) {
				for (int i = 0; i < textures.Length; i++) {
					detailPages [i] = Instantiate (pageMaterialPrefab);
					detailPages [i].mainTexture = (Texture2D)textures [i];
				}
				// Page 1's detail material is the second material.
				setPageMaterial (page1, detailPages [0], 1);
				currentViewingPage = 0;
			} else {
				setPageMaterial (page1, paperMaterial, 1);
			}
		}

		// End detail viewing mode.
		if (detailViewingMode && Input.GetAxisRaw("Cancel") > 0) {
			detailViewingMode = false;
			clipboardShowHideAnimator.Play ("Hide Clipboard");
			reticle.GetComponent<MeshRenderer> ().enabled = true;
			clickSign.GetComponent<MeshRenderer> ().enabled = true;
		}

		// Reprobe the reflection for the clipboard's clip.
		if (clipboardReflectionRefresh &&
					clipboardShowHideAnimator.GetCurrentAnimatorStateInfo(0).IsName("Show Clipboard") &&
					clipboardShowHideAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1) {
			clipboardReflectionRefresh = false;
			clipboard.GetComponentInChildren<ReflectionProbe> ().RenderProbe ();
		}

		// Refresh scrollability
		if (!scrollable && clipboardAnimator.GetCurrentAnimatorStateInfo (0).normalizedTime >= 1) {
			scrollable = true;
		}

		// Handle the detail page flipping.
		if (detailPages.Length > 0 && scrollable) {
			float mouseScroll = Input.GetAxisRaw ("Mouse ScrollWheel");
			if (mouseScroll < 0) { // Scroll down
				if (currentViewingPage < detailPages.Length - 1) {
					scrollable = false;
					currentViewingPage++;
					if (currentViewingPage == 1) {
						if (detailPages.Length > 1) {
							setPageMaterial(page2, detailPages[currentViewingPage], 1);
							clipboardAnimator.Play ("Page 1 flip", 0, 0);
						}
					} else if (currentViewingPage > 1) {
						if (detailPages.Length > 2) {
							setPageMaterial(page2, detailPages[currentViewingPage - 1], 1);
							setPageMaterial(page3, detailPages[currentViewingPage], 1);
							clipboardAnimator.Play ("Page 2 flip", 0, 0);
						}
					}
				}
			} else if (mouseScroll > 0) { // Scroll up
				scrollable = false;
				if (currentViewingPage > 0) {
					currentViewingPage--;
					if (currentViewingPage == 0) {
						if (detailPages.Length > 1) {
							clipboardAnimator.Play ("Page 1 flip reversed", 0, 0);
						}
					} else {
						if (detailPages.Length > 2) {
							setPageMaterial(page2, detailPages[currentViewingPage], 1);
							setPageMaterial(page3, detailPages[currentViewingPage + 1], 1);
							clipboardAnimator.Play ("Page 2 flip reversed", 0, 0);
						}
					}
				}
			}
		}
	}

	void setPageMaterial(GameObject gameObject, Material material, int index) {
		Material[] newMaterials = new Material[2];
		newMaterials [index] = material;
		newMaterials [1 - index] = paperMaterial; // The other material.
		gameObject.GetComponent<Renderer>().materials = newMaterials;
	}
}
