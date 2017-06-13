using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PictureFrameInfo : MonoBehaviour {
	public enum Wall {MainNorthLeft, MainNorthRight};

	public string name;
	public string id;
	public Wall wall;
	public string picturePath;

	public static string[] pictureFolder = { "Main Room North Left" };

	public string GetDetailsPath() {
		return "Picture frames/" + pictureFolder[(int) wall] + "/" + picturePath + "/" + "Details";
	}
}
