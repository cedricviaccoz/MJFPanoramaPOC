using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DomeFisheyeTexturer : MonoBehaviour {
	bool initialized = false;
	public DomeCameraRig cameraRig;

	void Initialize () {
		if (cameraRig == null)
			cameraRig = GameObject.FindObjectOfType<DomeCameraRig> ();
        if (cameraRig.renderTextures [0] == null)
			return;
		Debug.Log ("Initializing Fisheye Texture...");
		gameObject.GetComponent<MeshRenderer> ().material.shader = Shader.Find ("EMPlus/DomeFisheyeShader");
		gameObject.GetComponent<MeshRenderer> ().material.SetTexture ("_LeftTex", cameraRig.renderTextures [0]);
		gameObject.GetComponent<MeshRenderer> ().material.SetTexture ("_RightTex", cameraRig.renderTextures [1]);
		gameObject.GetComponent<MeshRenderer> ().material.SetTexture ("_TopTex", cameraRig.renderTextures [2]);
		gameObject.GetComponent<MeshRenderer> ().material.SetTexture ("_BottomTex", cameraRig.renderTextures [3]);
		gameObject.layer = DomeCameraRig.DOME_LAYER;
		initialized = true;
	}

	void Update () {
		if (!initialized)
			Initialize ();
	}
}
