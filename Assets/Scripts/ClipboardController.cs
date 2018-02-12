using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClipboardController : MonoBehaviour {
    public const int FLIPPING_MODE_NONE = 0;
    public const int FLIPPING_MODE_FORWARD = 1;
    public const int FLIPPING_MODE_BACKWARD = 2;
    public Animator clipboardAnimator;
	public Animator clipboardShowHideAnimator;
	public GameObject clipboardTarget;
	public GameObject clipboardContainer;
	public Material[] detailPages;
	public GameObject page1;
	public GameObject page2;
	public GameObject page3;
	public Material paperMaterial;
    [HideInInspector]
	public int currentViewingPage;
	public AudioSource bringUpClipboard;
	public AudioSource bringDownClipboard;
	public AudioSource flipForward;
	public AudioSource flipBackward;
    public GameObject pageGripContainer;
    public GameObject page1TailBone;
    public GameObject page2TailBone;
    public VRControllerCheck vrControllerCheck;
    [HideInInspector]
    public int controllerFlippingMode;
    [HideInInspector]
    public float flippingAlpha;
    private string pageReverseAnimation;

    // Use this for initialization
    void Start () {
        clipboardAnimator.Play ("Page 1 flip reversed", 0, 1);
        if (!vrControllerCheck.vrMode) {
            clipboardShowHideAnimator.Play("Hide Clipboard", 0, 1);
        } else {
            clipboardShowHideAnimator.Play("Show Clipboard", 0, 1);
        }
	}
	
	// Update is called once per frame
	void Update () {
		// Reprobe the reflection for the clipboard's clip.
		/*if (clipboardReflectionRefresh &&
			clipboardShowHideAnimator.GetCurrentAnimatorStateInfo(0).IsName("Show Clipboard") &&
			clipboardShowHideAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1) {
			clipboardReflectionRefresh = false;
		}*/

        if (controllerFlippingMode == FLIPPING_MODE_FORWARD) {
            if (currentViewingPage == 1) {
                if (detailPages.Length > 1) {
                    clipboardAnimator.Play("Page 1 flip", 0, flippingAlpha);
                    pageReverseAnimation = "Page 1 flip reversed";
                    gripToPage(page1TailBone);
                }
            } else if (currentViewingPage > 1) {
                if (detailPages.Length > 2) {
                    clipboardAnimator.Play("Page 2 flip", 0, flippingAlpha);
                    pageReverseAnimation = "Page 2 flip reversed";
                    gripToPage(page2TailBone);
                }
            }
        }

        if (controllerFlippingMode == FLIPPING_MODE_BACKWARD) {
            if (currentViewingPage == 0) {
                if (detailPages.Length > 1) {
                    clipboardAnimator.Play("Page 1 flip reversed", 0, flippingAlpha);
                    pageReverseAnimation = "Page 1 flip";
                    gripToPage(page1TailBone);
                }
            } else {
                if (detailPages.Length > 2) {
                    clipboardAnimator.Play("Page 2 flip reversed", 0, flippingAlpha);
                    pageReverseAnimation = "Page 2 flip";
                    gripToPage(page2TailBone);
                }
            }
        }
	}

    public void FinalizeFlipping() {
        if (flippingAlpha < .5) {
            if (controllerFlippingMode == FLIPPING_MODE_FORWARD) {
                FlipBackward();
            } else if (controllerFlippingMode == FLIPPING_MODE_BACKWARD) {
                FlipForward();
            }
            clipboardAnimator.Play(pageReverseAnimation, 0, 1 - flippingAlpha);
        }
    }

    private void gripToPage(GameObject pageTailBone) {
        pageGripContainer.transform.position = pageTailBone.transform.position;
        pageGripContainer.transform.rotation = pageTailBone.transform.rotation;
    }

	public void Show() {
        if (!vrControllerCheck.vrMode) {
            clipboardShowHideAnimator.Play("Show Clipboard");
            clipboardAnimator.Play("Page 1 flip reversed", 0, 1);
            clipboardContainer.transform.position = clipboardTarget.transform.position;
            if (detailPages.Length > 0) {
                setPageMaterial(page1, detailPages[0], 1);
            } else {
                setPageMaterial(page1, paperMaterial, 1);
            }
            bringUpClipboard.Play();
        } else {
            clipboardAnimator.Play("Page 1 flip reversed", 0, 1);
            if (detailPages.Length > 0) {
                setPageMaterial(page1, detailPages[0], 1);
            } else {
                setPageMaterial(page1, paperMaterial, 1);
            }
        }
	}

    public bool CanFlipForward() {
        return currentViewingPage < detailPages.Length - 1;
    }

    public bool CanFlipBackward() {
        return currentViewingPage > 0;
    }

    public void ShowAtTouchController() {
        clipboardShowHideAnimator.Play("Show Clipboard", 0, 1);
    }

	public void Hide() {
        if (!vrControllerCheck.vrMode) {
            clipboardShowHideAnimator.Play("Hide Clipboard");
            bringDownClipboard.Play();
        }
	}

	public void FlipForward() {
        if (CanFlipForward()) {
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
			flipForward.Play ();
		}
	}

	public void FlipBackward() {
        if (CanFlipBackward()) {
			currentViewingPage--;
			if (currentViewingPage == 0) {
				if (detailPages.Length > 1) {
					clipboardAnimator.Play ("Page 1 flip reversed", 0, 0);
				}
			} else {
				if (detailPages.Length > 2) {
					setPageMaterial (page2, detailPages [currentViewingPage], 1);
					setPageMaterial (page3, detailPages [currentViewingPage + 1], 1);
					clipboardAnimator.Play ("Page 2 flip reversed", 0, 0);
				}
			}
			flipBackward.Play ();
		}
	}

	void setPageMaterial(GameObject gameObject, Material material, int index) {
		Material[] newMaterials = new Material[2];
		newMaterials [index] = material;
		newMaterials [1 - index] = paperMaterial; // The other material.
		gameObject.GetComponent<Renderer>().materials = newMaterials;
	}
}
