using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class DomeCameraRig : MonoBehaviour {
    public const int DOME_LAYER = 31;
    [ReadOnlyWhenPlaying] public int cubemapSize = 2048;
    [ReadOnlyWhenPlaying] public bool debug = false;
    [ReadOnlyWhenPlaying] public bool renderTop = true;
    [ReadOnlyWhenPlaying] public bool renderBottom = true;

    [NonSerialized][HideInInspector] public GameObject cameraRig;
    [NonSerialized][HideInInspector] public GameObject[] cameraGOs;
    [NonSerialized][HideInInspector] public Camera[] cameras;
    [NonSerialized][HideInInspector] public RenderTexture[] renderTextures;
    [NonSerialized][HideInInspector] public GameObject debugRig;
    [NonSerialized][HideInInspector] public bool isInitialized = false;

    private Camera originalCam;
    private GameObject[] debugQuads;

    void Start () {
        Debug.Log("Setting up Dome Camera Rig (debug: "+(debug?"yes":"no")+")");
        originalCam = gameObject.GetComponent<Camera> ();
        SetupCameraRig ();
        ConfigureOriginalCam ();
        isInitialized = true;
    }
	
    void ConfigureOriginalCam () {
        originalCam.cullingMask = (1 << DOME_LAYER);
        originalCam.clearFlags = CameraClearFlags.SolidColor;
        if (debug)
            originalCam.backgroundColor = Color.blue;
        else
            originalCam.backgroundColor = Color.black;
    }

    void SetupCameraRig () {
        cameraRig = new GameObject("Dome Camera Rig");
        cameraRig.transform.position = transform.position;
        cameraRig.transform.rotation = transform.rotation;
        cameraRig.transform.SetParent (transform);

        cameraGOs = new GameObject[4];
        cameras = new Camera[4];
        renderTextures = new RenderTexture[4];

        for (int n = 0; n < 4; n++) {
            if (n == 2 && !renderTop) continue;
            if (n == 3 && !renderBottom) continue;
            renderTextures [n] = new RenderTexture (cubemapSize, cubemapSize, 16);
            renderTextures [n].name = "Dome Cube Render Texture " + n;

            cameraGOs [n] = new GameObject ("Dome Cube Cam " + n);
            cameraGOs [n].transform.position = cameraRig.transform.position;
            cameraGOs [n].transform.rotation = cameraRig.transform.rotation;
            cameraGOs [n].transform.SetParent (cameraRig.transform);
            switch (n) {
                case 0: cameraGOs [n].transform.Rotate (new Vector3 (0f, -45f, 0f));
                    break;
                case 1: cameraGOs [n].transform.Rotate (new Vector3 (0f, 45f, 0f));
                    break;
                case 2: cameraGOs [n].transform.Rotate (new Vector3 (-90f, -45f, 0f));
                    break;
                case 3: cameraGOs [n].transform.Rotate (new Vector3 (90f, 45f, 0f));
                    break;
            }
            cameras [n] = cameraGOs [n].AddComponent<Camera> ();
            cameras [n].targetTexture = renderTextures [n];
            cameras [n].fieldOfView = 90;
            cameras [n].clearFlags = originalCam.clearFlags;
            cameras [n].backgroundColor = originalCam.backgroundColor;
            cameras [n].cullingMask = originalCam.cullingMask & 0x7fffffff;
            cameras [n].pixelRect = new Rect (0f, 0f, cubemapSize, cubemapSize);
        }

        // add debugging quads / hide cameraRig
        if (!debug)
            cameraRig.hideFlags = HideFlags.HideAndDontSave;
        else {
            debugRig = new GameObject ("Dome Debug Rig");
            debugRig.transform.position = transform.position;
            debugRig.transform.rotation = transform.rotation;
            debugRig.transform.SetParent (transform);
            debugQuads = new GameObject[4];
        }

        for (int n = 0; n < 4; n++) {
            if (n == 2 && !renderTop) continue;
            if (n == 3 && !renderBottom) continue;
            if (!debug)
                cameraGOs [n].hideFlags = HideFlags.HideAndDontSave;
            else {
                var sqrt2 = Mathf.Sqrt (2f);
                debugQuads [n] = GameObject.CreatePrimitive (PrimitiveType.Quad);
                debugQuads [n].name = "Dome Cube Debug Quad " + n;
                debugQuads [n].GetComponent<MeshRenderer> ().material.SetTexture ("_MainTex", renderTextures [n]);
                debugQuads [n].GetComponent<MeshRenderer> ().material.shader = Shader.Find ("Unlit/Texture");
                debugQuads [n].transform.SetParent (debugRig.transform);
                debugQuads [n].layer = DOME_LAYER;

                switch (n) {
                    case 0:
                        debugQuads [n].transform.Rotate (new Vector3 (0f, -45f, 0f));
                        debugQuads [n].transform.position += new Vector3 (-0.5f / sqrt2, 0f, 0.5f / sqrt2);
                        break;
                    case 1:
                        debugQuads [n].transform.Rotate (new Vector3 (0f, 45f, 0f));
                        debugQuads [n].transform.position += new Vector3 (0.5f / sqrt2, 0f, 0.5f / sqrt2);
                        break;
                    case 2:
                        debugQuads [n].transform.Rotate (new Vector3 (-90f, -45f, 0f));
                        debugQuads [n].transform.position += new Vector3 (0f, 0.5f, -0.0f / sqrt2);
                        break;
                    case 3:
                        debugQuads [n].transform.Rotate (new Vector3 (90f, 45f, 0f));
                        debugQuads [n].transform.position += new Vector3 (0f, -0.5f, -0.0f / sqrt2);
                        break;
                }
            }
        }
    }
}
