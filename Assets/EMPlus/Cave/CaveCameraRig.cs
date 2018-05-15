using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class CaveCameraRig : MonoBehaviour {
	public const int CAVE_LAYER = 31;
	[ReadOnlyWhenPlaying] public bool debug = false;
	[ReadOnlyWhenPlaying] public bool renderToTexture = false;
	[ReadOnlyWhenPlaying] public Vector2Int textureSize = new Vector2Int(2560, 1600);
	public bool leftEye = true;
	public float interOccularDistance = 0.065f;
	public Vector2 screenSize = new Vector2 (3.2f, 2.0f);
	public Vector3 viewerPosition = new Vector3 (1.6f, 1.65f, 0.9f); 

	[NonSerialized][HideInInspector] public GameObject cameraRig;
	[NonSerialized][HideInInspector] public GameObject[] cameraGOs;
	[NonSerialized][HideInInspector] public Camera[] cameras;
	[NonSerialized][HideInInspector] public RenderTexture[] renderTextures;
	[NonSerialized][HideInInspector] public GameObject debugRig;
	[NonSerialized][HideInInspector] public bool isInitialized = false;

	private Camera originalCam;
	private GameObject[] debugQuads;

	void Start () {
		Debug.Log("Setting up Cave Camera Rig (debug: "+(debug?"yes":"no")+")");
		originalCam = gameObject.GetComponent<Camera> ();
		SetupCameraRig ();
		ConfigureOriginalCam ();
		isInitialized = true;
		OnValidate ();
	}

	void ConfigureOriginalCam () {
		originalCam.cullingMask = (1 << CAVE_LAYER);
		originalCam.clearFlags = CameraClearFlags.SolidColor;
		if (debug)
			originalCam.backgroundColor = Color.blue;
		else
			originalCam.backgroundColor = Color.black;
	}

	void SetupCameraRig () {
		cameraRig = new GameObject("Cave Camera Rig");
		cameraRig.transform.position = transform.position;
		cameraRig.transform.rotation = transform.rotation;
		cameraRig.transform.SetParent (transform);

		cameraGOs = new GameObject[2];
		cameras = new Camera[2];
		if (renderToTexture)
			renderTextures = new RenderTexture[2];

		for (int n = 0; n < 2; n++) {
			cameraGOs [n] = new GameObject ("Cave Cam " + n);
			float sx = interOccularDistance * (leftEye ? -0.5f : 0.5f);
			cameraGOs [n].transform.position = cameraRig.transform.position + new Vector3 (sx, 0, 0);
			cameraGOs [n].transform.rotation = cameraRig.transform.rotation;
			cameraGOs [n].transform.SetParent (cameraRig.transform);
			cameras [n] = cameraGOs [n].AddComponent<Camera> ();
			cameras [n].clearFlags = originalCam.clearFlags;
			cameras [n].backgroundColor = originalCam.backgroundColor;
			cameras [n].cullingMask = originalCam.cullingMask & 0x7fffffff;
			switch (n) {
			case 0:
				cameraGOs [n].transform.Rotate (new Vector3 (0f, 0f, 0f));
				cameras [n].pixelRect = new Rect (0f, originalCam.pixelRect.height / 2, originalCam.pixelRect.width, originalCam.pixelRect.height / 2);
				break;
			case 1:
				cameraGOs [n].transform.Rotate (new Vector3 (90f, 0f, 0f));
				cameras [n].pixelRect = new Rect (0f, 0f, originalCam.pixelRect.width, originalCam.pixelRect.height / 2);
				break;
			}
			if (renderToTexture) {
				renderTextures [n] = new RenderTexture (textureSize.x, textureSize.y, 24);
//				cameras [n].targetTexture = renderTextures [n];
				cameras[n].SetTargetBuffers(renderTextures[n].colorBuffer, renderTextures[n].depthBuffer);
				cameras [n].pixelRect = new Rect (0f, 0f, textureSize.x, textureSize.y);
			}
		}

		// add debugging quads / hide cameraRig
		if (!debug)
			cameraRig.hideFlags = HideFlags.HideAndDontSave;
		else {
			debugRig = new GameObject ("Cave Debug Rig");
			debugRig.transform.position = transform.position;
			debugRig.transform.rotation = transform.rotation;
			debugRig.transform.SetParent (transform);
			debugQuads = new GameObject[2];
		}

		for (int n = 0; n < 2; n++) {
			if (!debug)
				cameraGOs [n].hideFlags = HideFlags.HideAndDontSave;
			else {
				debugQuads [n] = GameObject.CreatePrimitive (PrimitiveType.Quad);
				debugQuads [n].name = "Cave Cube Debug Quad " + n;
				debugQuads [n].transform.SetParent (debugRig.transform);
				debugQuads [n].layer = CAVE_LAYER;
				switch (n) {
				case 0:
					debugQuads [n].transform.Rotate (new Vector3 (0f, 0f, 0f));
					debugQuads [n].transform.position += new Vector3 (0f, 0f, 1f);
					break;
				case 1:
					debugQuads [n].transform.Rotate (new Vector3 (90f, 0f, 0f));
					debugQuads [n].transform.position += new Vector3 (0f, -1f, 0f);
					break;
				}
				if (renderToTexture) {
					debugQuads [n].GetComponent<MeshRenderer> ().material.SetTexture ("_MainTex", renderTextures [n]);
					debugQuads [n].GetComponent<MeshRenderer> ().material.shader = Shader.Find ("Unlit/Texture");
				}
			}
		}
	}

	void OnValidate()
	{
		if (debug) {
			for (int n = 0; n < 2; n++) {
				debugQuads [n].transform.localScale = new Vector3 (screenSize.x, screenSize.y, 1f);
				debugQuads [n].transform.position = transform.position + (n == 0 ? new Vector3 (0f, 0f, screenSize.y * 0.5f) : new Vector3 (0f, -screenSize.y * 0.5f, 0f));
				debugQuads [n].transform.position += new Vector3(screenSize.x * 0.5f, screenSize.y * 0.5f, screenSize.y * 0.5f) - viewerPosition;
			}
		}
		float sx = interOccularDistance * (leftEye ? -0.5f : 0.5f);
		float shiftx = (sx + viewerPosition.x - screenSize.x * 0.5f) / screenSize.x;
		for (int n = 0; n < 2; n++) {
			cameraGOs [n].transform.position = cameraRig.transform.position + new Vector3 (sx, 0, 0);
			cameras [n].ResetProjectionMatrix ();
			if (n == 0) {
				cameras [n].fieldOfView = 2f * Mathf.Rad2Deg * Mathf.Atan2 (screenSize.y * 0.5f, screenSize.y - viewerPosition.z);
			} else {
				cameras [n].fieldOfView = 2f * Mathf.Rad2Deg * Mathf.Atan2 (screenSize.y * 0.5f, viewerPosition.y);
			}
			float shifty = ((n == 0 ? viewerPosition.y : viewerPosition.z) - screenSize.y * 0.5f) / screenSize.y;
			Matrix4x4 mat = cameras[n].projectionMatrix;
			mat[0, 2] = -shiftx * 2f;
			mat[1, 2] = -shifty * 2f;
			cameras[n].projectionMatrix = mat;
		}
	}
}