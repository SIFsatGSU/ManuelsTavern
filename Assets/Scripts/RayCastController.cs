using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class RayCastController : MonoBehaviour {
    public Camera playerCamera;
    public GameObject UIElement;
    public GameObject reticle;
    public GameObject clickSign;
    public GameObject clipboardContainer;
    public GameObject rightHand;
    public GameObject rightHandModel;
    public GameObject leftHand;
    public Material pageMaterialPrefab;
    public float UIElementSize;
    public float clickSignAlphaSpeed;
    public Animator clipboardAnimator;
    public float linearInputThreshold;
    public float grippingRadius;
    public GameObject pageGripPoint;
    [HideInInspector]
    public bool oculusControllerMode;
    [HideInInspector]
    public bool detailViewingMode = false;
    private ClipboardController clipboardController;
    private Hashtable pictureFolderMap = new Hashtable();
    private string pictureLookedAt = "";
    private string wallLookedAt = "";
    private float clickSignAlpha = 0;
    private bool scrollable = true;
    private GameObject clipboard;
    private GameObject leftHandPoint;
    private GameObject rightHandPoint;
    private GameObject rightHandModelPoint;
    private GameObject clipboardHoldingPoint;
    private GameObject clipboardBottomPoint;
    private GameObject clipboardTopPoint;
    private bool rightHandTrigger = false;
	private bool leftHandTrigger = false;

	// Use this for initialization
	void Start () {
        pictureFolderMap["Pictures Main Room Left"] = "Main Room North Left";
        leftHandPoint = leftHand.transform.GetChild(0).gameObject;
        rightHandPoint = rightHand.transform.GetChild(0).gameObject;
        rightHandModelPoint = rightHandModel.transform.GetChild(1).gameObject;
        clipboard = clipboardContainer.transform.GetChild(0).gameObject;
        clipboardController = clipboard.GetComponent<ClipboardController>();
        clipboardHoldingPoint = clipboardContainer.transform.GetChild(1).gameObject;
        clipboardBottomPoint = clipboardContainer.transform.GetChild(2).gameObject;
        clipboardTopPoint = clipboardContainer.transform.GetChild(3).gameObject;
    }
	
	// Update is called once per frame
	void Update () {
        // Ray cast
        Ray forwardRay;
        if (!oculusControllerMode) {
            forwardRay = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        } else { // Cast ray from right hand if there's a touch controller.
            forwardRay = new Ray(rightHand.transform.position, rightHand.transform.forward);
        }
        
        Debug.DrawRay(forwardRay.origin, forwardRay.direction);
        RaycastHit hit;
        bool showingClickSign = false;
        pictureLookedAt = "";
        if (Physics.Raycast(forwardRay, out hit)) {
            Vector3 hitProjection = hit.point - playerCamera.transform.position;
            float distance = hitProjection.magnitude;
            //UIElement.transform.localPosition = new Vector3(0, 0, distance);
            UIElement.transform.position = hit.point;
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
        Color reticleColor = reticle.GetComponent<Renderer>().material.color;
        //reticle.GetComponent<Renderer> ().material.color = new Color(reticleColor.r, reticleColor.g, reticleColor.b, clickSignAlpha);
        clickSign.GetComponent<TextMesh>().color = new Color(1, 1, 1, clickSignAlpha);

        if (!oculusControllerMode) { // Mouse and keyboard (or controller) mode.
            // Start detail viewing mode.
            if (!detailViewingMode && Input.GetAxisRaw("Use") > 0 && pictureLookedAt != "") {
                detailViewingMode = true;
                reticle.GetComponent<MeshRenderer>().enabled = false;
                clickSign.GetComponent<MeshRenderer>().enabled = false;
                playerCamera.transform.localEulerAngles = new Vector3(0, 0, 0);
                //clipboardContainer.transform.LookAt (playerCamera.transform.position);
                loadClipboardContent();
                clipboard.GetComponent<ClipboardController>().Show();
                GetComponentInChildren<DepthOfField>().enabled = true;
            }

            // End detail viewing mode.
            if (detailViewingMode && Input.GetAxisRaw("Cancel") > 0) {
                detailViewingMode = false;
                clipboardController.Hide();
                reticle.GetComponent<MeshRenderer>().enabled = true;
                clickSign.GetComponent<MeshRenderer>().enabled = true;
                GetComponentInChildren<DepthOfField>().enabled = false;
            }

            // Refresh scrollability
            if (!scrollable && clipboardAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1) {
                scrollable = true;
            }

            // Handle the detail page flipping.
            if (detailViewingMode && clipboardController.detailPages.Length > 0 && scrollable) {
                float scroll = Input.GetAxisRaw("Details Scroll");
                if (scroll < 0) { // Scroll down
                    scrollable = false;
                    clipboardController.FlipForward();
                } else if (scroll > 0) { // Scroll up
                    scrollable = false;
                    clipboardController.FlipBackward();
                }
            }
        } else { // Oculus Rift Touch controller mode.
            OVRHandController rightHandController = rightHand.GetComponent<OVRHandController>();
            OVRHandController leftHandController = leftHand.GetComponent<OVRHandController>();

            float rightFistLevel = OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger);
            bool rightTriggerTouch = OVRInput.Get(OVRInput.RawNearTouch.RIndexTrigger);
            float rightTriggerLevel = OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger);
            bool rightThumb = OVRInput.Get(OVRInput.RawNearTouch.RThumbButtons);

            float leftFistLevel = OVRInput.Get(OVRInput.RawAxis1D.LHandTrigger);
            bool leftTriggerTouch = OVRInput.Get(OVRInput.RawNearTouch.LIndexTrigger);
            float leftTriggerLevel = OVRInput.Get(OVRInput.RawAxis1D.LIndexTrigger);
            bool leftThumb = OVRInput.Get(OVRInput.RawNearTouch.LThumbButtons);

            animateHand(rightHandController, rightFistLevel, rightTriggerTouch, rightTriggerLevel, rightThumb);
            animateHand(leftHandController, leftFistLevel, leftTriggerTouch, leftTriggerLevel, leftThumb);

			bool currentLeftHandTrigger = leftTriggerLevel > linearInputThreshold;
			if (!leftHandTrigger && currentLeftHandTrigger) { // Start holding clipboard.
                clipboardController.ShowAtTouchController();
                snapObjectToPoint(clipboardContainer, clipboardHoldingPoint, leftHandPoint);
                clipboardContainer.GetComponent<GrabMotionTrack>().enable = true;
                clipboard.GetComponentInChildren<Light>().enabled = true; // Turn on the clipboard light.
                leftHand.GetComponent<OVRHandController>().thumbTarget = .9f;
				leftHand.GetComponent<OVRHandController>().fistTarget = 1;
				clipboardContainer.GetComponentInChildren<ReflectionProbe> ().RenderProbe ();
			} if (leftHandTrigger && currentLeftHandTrigger) { // Holding clipboard
				clipboardController.ShowAtTouchController();
				snapObjectToPoint(clipboardContainer, clipboardHoldingPoint, leftHandPoint);
				leftHand.GetComponent<OVRHandController>().thumbTarget = .9f;
				leftHand.GetComponent<OVRHandController>().fistTarget = 1;
			} else if (leftHandTrigger && !currentLeftHandTrigger) {
                clipboardContainer.GetComponent<GrabMotionTrack>().enable = false;
                clipboard.GetComponentInChildren<Light>().enabled = false; // Turn off the clipboard light.
            }
			leftHandTrigger = currentLeftHandTrigger;

            bool currentRightHandTrigger = rightTriggerLevel > linearInputThreshold;
            if (!rightHandTrigger && currentRightHandTrigger) { // Just pulled trigger.
                if ((rightHandPoint.transform.position - clipboardBottomPoint.transform.position).sqrMagnitude < grippingRadius * grippingRadius) {
                    if (clipboardController.CanFlipForward()) {
                        clipboardController.controllerFlippingMode = ClipboardController.FLIPPING_MODE_FORWARD;
                        clipboardController.FlipForward();
                    }
                } else if ((rightHandPoint.transform.position - clipboardTopPoint.transform.position).sqrMagnitude < grippingRadius * grippingRadius) {
                    if (clipboardController.CanFlipBackward()) {
                        clipboardController.controllerFlippingMode = ClipboardController.FLIPPING_MODE_BACKWARD;
                        clipboardController.FlipBackward();
                    }
                } else if (pictureLookedAt != "") {
                    loadClipboardContent();
                    clipboardController.Show();
                }
            } else if (rightHandTrigger && currentRightHandTrigger) {
                if (clipboardController.controllerFlippingMode != ClipboardController.FLIPPING_MODE_NONE) {
                    float distanceFromBottom = distance(rightHandPoint.transform.position, clipboardBottomPoint.transform.position);
                    float distanceFromTop = distance(rightHandPoint.transform.position, clipboardTopPoint.transform.position);
                    float alpha = distanceFromBottom / (distanceFromBottom + distanceFromTop);
                    if (clipboardController.controllerFlippingMode == ClipboardController.FLIPPING_MODE_BACKWARD) {
                        alpha = 1 - alpha;
                    }
                    clipboardController.flippingAlpha = alpha;
                    rightHand.GetComponent<OVRHandController>().snapHand = false;
                    snapObjectToPoint(rightHandModel, rightHandModelPoint, pageGripPoint);
                    animateHand(rightHandController, 1, true, 1, true);
                }
            } else if (rightHandTrigger && !currentRightHandTrigger) {
                rightHand.GetComponent<OVRHandController>().snapHand = true;
                if (clipboardController.controllerFlippingMode != ClipboardController.FLIPPING_MODE_NONE) {
                    clipboardController.FinalizeFlipping();
                }
                clipboardController.controllerFlippingMode = ClipboardController.FLIPPING_MODE_NONE;
            }

            rightHandTrigger = currentRightHandTrigger;
        }
	}

    private void snapObjectToPoint(GameObject sourceObject, GameObject snappingPoint, GameObject target) {
        sourceObject.transform.rotation = target.transform.rotation;
        sourceObject.transform.position = target.transform.position;
        sourceObject.transform.position -= snappingPoint.transform.position - sourceObject.transform.position;
    }

    private float distance(Vector3 a, Vector3 b) {
        return (a - b).magnitude;
    }

    void loadClipboardContent() {
        // Initiate the detail pages materials.
        string path = "Picture frames/" + pictureFolderMap[wallLookedAt] + "/" + pictureLookedAt + "/" + "Details";
		print (path);
        Object[] textures = Resources.LoadAll(path);
        clipboardController.detailPages = new Material[textures.Length];
        if (textures.Length > 0) {
            for (int i = 0; i < textures.Length; i++) {
                clipboardController.detailPages[i] = Instantiate(pageMaterialPrefab);
                clipboardController.detailPages[i].mainTexture = (Texture2D)textures[i];
            }
            // Page 1's detail material is the second material.
            clipboardController.currentViewingPage = 0;
        }
    }

    void animateHand(OVRHandController controller, float fist, bool triggerTouch, float trigger, bool thumb) {
        controller.fistTarget = fist;
        if (triggerTouch) {
            if (trigger > 0) {
                controller.indexTarget = Mathf.Lerp(.55f, 1f, trigger);
            } else {
                controller.indexTarget = .5f;
            }
        } else {
            controller.indexTarget = 0;
        }
        controller.thumbTarget = thumb ? 1 : 0;
    }
}
