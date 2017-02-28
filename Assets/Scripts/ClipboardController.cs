using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClipboardController : MonoBehaviour {
	public Animator clipboardAnimator;
	public Animator clipboardShowHideAnimator;
	public GameObject clipboardTarget;
	public GameObject clipboardContainer;
	public Material[] detailPages;
	public GameObject page1;
	public GameObject page2;
	public GameObject page3;
	public Material paperMaterial;
	public int currentViewingPage;
	public AudioSource bringUpClipboard;
	public AudioSource bringDownClipboard;
	public AudioSource flipForward;
	public AudioSource flipBackward;
	private bool clipboardReflectionRefresh;

	// Use this for initialization
	void Start () {
		clipboardAnimator.Play ("Page 1 flip reversed", 0, 1);
		clipboardShowHideAnimator.Play ("Hide Clipboard", 0, 1);
	}
	
	// Update is called once per frame
	void Update () {
		// Reprobe the reflection for the clipboard's clip.
		if (clipboardReflectionRefresh &&
			clipboardShowHideAnimator.GetCurrentAnimatorStateInfo(0).IsName("Show Clipboard") &&
			clipboardShowHideAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1) {
			clipboardReflectionRefresh = false;
			GetComponentInChildren<ReflectionProbe> ().RenderProbe ();
		}
	}

	public void Show() {
		clipboardShowHideAnimator.Play ("Show Clipboard");
		clipboardAnimator.Play ("Page 1 flip reversed", 0, 1);
		clipboardReflectionRefresh = true;
		clipboardContainer.transform.position = clipboardTarget.transform.position;
		if (detailPages.Length > 0) {
			setPageMaterial (page1, detailPages [0], 1);
		} else {
			setPageMaterial (page1, paperMaterial, 1);
		}
		bringUpClipboard.Play ();
	}

	public void Hide() {
		clipboardShowHideAnimator.Play ("Hide Clipboard");
		bringDownClipboard.Play ();
	}

	public void FlipForward() {
		if (currentViewingPage < detailPages.Length - 1) {
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
		if (currentViewingPage > 0) {
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
