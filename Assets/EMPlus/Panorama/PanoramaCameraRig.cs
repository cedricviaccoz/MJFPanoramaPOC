using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class PanoramaCameraRig : MonoBehaviour {
    public const int PANORAMA_LAYER = 31;

	[ReadOnlyWhenPlaying] public bool detachFromCamera = false;
	[ReadOnlyWhenPlaying] public bool debug = false;
	public bool undistortDebugSlices = true;

    [ReadOnlyWhenPlaying] public int screenSlices = 18;
    public float startAngle = 0;
    public float endAngle = 180;
    public Vector2Int screenResolution = new Vector2Int(8192, 1000);
    public bool leftEye = true;
    public float interOccularDistance = 0.065f;
    public float convergenceDistance = 1f;
    public float cameraShiftX = 0;
    public float cameraShiftY = 0;
    public float toeAngle = 0;

    [NonSerialized] [HideInInspector] public GameObject cameraRig;
    [NonSerialized] [HideInInspector] public GameObject[] cameraGOs;
    [NonSerialized] [HideInInspector] public Camera[] cameras;
    [NonSerialized] [HideInInspector] public RenderTexture renderTexture;
    [NonSerialized] [HideInInspector] public GameObject debugRig;
    [NonSerialized] [HideInInspector] public bool isInitialized = false;

	[NonSerialized] [HideInInspector] public int numSlices = 0;
	[NonSerialized] [HideInInspector] public float sliceAngle = 10;
    private Camera originalCam;
    private GameObject[] debugQuads;
    private float startSliceAngle = 0;
    private float endSliceAngle = 0;
    private Vector2Int sliceSizePx;
    private GameObject debugQuad;
    private GameObject leftBlind, rightBlind;

	public float RenderedAngle() {
		return (endSliceAngle - startSliceAngle);
	}

    void Start()
    {
        Debug.Log("Setting up Panorama Camera Rig (debug: " + (debug ? "yes" : "no") + ")");
        originalCam = gameObject.GetComponent<Camera>();
        SetupCameraRig();
        ConfigureOriginalCam();
        if (debug)
        {
            CreateDebugQuad();
        }
        isInitialized = true;
    }

    void ConfigureOriginalCam()
    {
        originalCam.cullingMask = (1 << PANORAMA_LAYER);
        originalCam.clearFlags = CameraClearFlags.SolidColor;
        if (debug)
            originalCam.backgroundColor = Color.blue;
        else
            originalCam.backgroundColor = Color.black;
    }
    void SetupCameraRig()
    {
        sliceAngle = 360f / (float)screenSlices;
        int startSlice = Mathf.FloorToInt(startAngle / sliceAngle);
        int endSlice = Mathf.CeilToInt(endAngle / sliceAngle);
        numSlices = endSlice - startSlice;
        startSliceAngle = ((float)startSlice * sliceAngle);
        endSliceAngle = ((float)endSlice * sliceAngle);
        cameraRig = new GameObject("Panorama Camera Rig");
        cameraRig.transform.position = transform.position;
        cameraRig.transform.rotation = transform.rotation;
		if (!detachFromCamera)
	        cameraRig.transform.SetParent(transform);

        int sliceWidth = screenResolution.x / screenSlices;
        sliceSizePx = new Vector2Int(sliceWidth, screenResolution.y);

        Debug.Log("Rig with " + numSlices + " slices (px: " + sliceWidth + "x" + screenResolution.y + " angle: " + startSliceAngle + " to " + endSliceAngle + ")");

        cameraGOs = new GameObject[numSlices];
        cameras = new Camera[numSlices];

        renderTexture = new RenderTexture(sliceSizePx.x * numSlices, sliceSizePx.y, 24);
        renderTexture.name = "Panorama Render Texture";
        for (int n = 0; n < numSlices; n++)
        {
            cameraGOs[n] = new GameObject("Panorama Cam " + n);
            cameraGOs[n].transform.position = cameraRig.transform.position;
            cameraGOs[n].transform.rotation = cameraRig.transform.rotation;
            cameraGOs[n].transform.SetParent(cameraRig.transform);
            cameras[n] = cameraGOs[n].AddComponent<Camera>();
            // cameras[n].targetTexture = renderTexture;
			cameras[n].SetTargetBuffers(renderTexture.colorBuffer, renderTexture.depthBuffer);

            cameras[n].clearFlags = originalCam.clearFlags;
            cameras[n].backgroundColor = originalCam.backgroundColor;
            cameras[n].cullingMask = originalCam.cullingMask ^ (1 << PANORAMA_LAYER);
            cameras[n].nearClipPlane = originalCam.nearClipPlane;
            cameras[n].farClipPlane = originalCam.farClipPlane;
        }
        UpdateCameras();
    }
    void UpdateCameras()
    {
        for (int n = 0; n < numSlices; n++)
        {
            float pairAngle = sliceAngle * ((float)n + 0.5f); // +0.5 so that left border of image matches start angle of slice
            cameras[n].transform.localRotation = Quaternion.identity;
            cameras[n].transform.Rotate(0f, pairAngle + startSliceAngle, 0);
            cameras[n].transform.localPosition = Vector3.zero;
            cameras[n].transform.Translate((leftEye ? -interOccularDistance : interOccularDistance) * 0.5f, 0f, 0f);
            cameras[n].transform.Rotate(0f, (leftEye ? toeAngle : -toeAngle), 0);
            cameras[n].pixelRect = new Rect(sliceSizePx.x * n, 0f, sliceSizePx.x, sliceSizePx.y);
            cameras[n].fieldOfView = VertFov(sliceAngle, cameras[n].aspect);

            float imgw = convergenceDistance * Mathf.Tan(sliceAngle * 0.5f * Mathf.Deg2Rad);
            float shiftx = interOccularDistance / (2f * imgw) + cameraShiftX;
            Matrix4x4 mat = cameras[n].projectionMatrix;
            mat[0, 2] = (leftEye ? shiftx : -shiftx);
            mat[1, 2] = cameraShiftY / 100f;
            cameras[n].projectionMatrix = mat;
        }
    }

    void UpdateDebug()
    {
        if (!leftBlind) return;
		if (undistortDebugSlices) {
			debugQuad.GetComponent<MeshRenderer> ().material.shader = Shader.Find ("EMPlus/PanoramaSliceWarpShader");
			debugQuad.GetComponent<MeshRenderer> ().material.SetVector ("_SliceLayout", new Vector4 (numSlices, sliceAngle * Mathf.Deg2Rad, sliceSizePx.x, sliceSizePx.y));
            debugQuad.GetComponent<MeshRenderer>().material.SetFloat("_ShiftY", cameraShiftY / 200f);
		} else {
			debugQuad.GetComponent<MeshRenderer> ().material.shader = Shader.Find ("Unlit/Texture");
		}
        float lp = (startAngle - startSliceAngle) / (endSliceAngle - startSliceAngle);
        float rp = (endAngle - startSliceAngle) / (endSliceAngle - startSliceAngle);
        leftBlind.transform.localPosition = new Vector3(lp - 1f, 0, 0.999f);
        rightBlind.transform.localPosition = new Vector3(rp, 0, 0.999f);
    }

    void OnValidate()
    {
        UpdateCameras();
        if (debug) UpdateDebug();
    }

        float VertFov(float horiFov, float aspect)
    {
        float hFOVrad = horiFov * Mathf.Deg2Rad;
        float camH = Mathf.Tan(hFOVrad * .5f) / aspect;
        float vFOVrad = Mathf.Atan(camH) * 2f;
        return vFOVrad * Mathf.Rad2Deg;
    }

    void CreateDebugQuad()
    {
        debugQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        debugQuad.name = "Panorama Debug Quad";
        debugQuad.transform.SetParent(transform);
        debugQuad.transform.position = gameObject.transform.position;
        debugQuad.transform.rotation = gameObject.transform.rotation;
        debugQuad.transform.Translate(0f, 0f, 1f);
        debugQuad.layer = PANORAMA_LAYER;
		if (undistortDebugSlices)
	        debugQuad.GetComponent<MeshRenderer>().material.shader = Shader.Find("EMPlus/PanoramaSliceWarpShader");
		else
			debugQuad.GetComponent<MeshRenderer>().material.shader = Shader.Find("Unlit/Texture");
        debugQuad.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", renderTexture);

		float sw = sliceSizePx.x * numSlices;
        float sh = screenResolution.y;
        if (sw < sh)
            debugQuad.transform.localScale = new Vector3(1, 1, 1);// new Vector3(sw / sh, 1, 1);
        else
            debugQuad.transform.localScale = new Vector3(1, sh / sw, 1);

        leftBlind = GameObject.CreatePrimitive(PrimitiveType.Quad);
        leftBlind.name = "Panorama Debug Left Clip Quad";
        leftBlind.transform.SetParent(transform);
        leftBlind.transform.position = gameObject.transform.position;
        leftBlind.transform.rotation = gameObject.transform.rotation;
        leftBlind.transform.Translate(-1f, 0f, 0.999f);
        leftBlind.layer = PANORAMA_LAYER;

        rightBlind = GameObject.CreatePrimitive(PrimitiveType.Quad);
        rightBlind.name = "Panorama Debug Left Clip Quad";
        rightBlind.transform.SetParent(transform);
        rightBlind.transform.position = gameObject.transform.position;
        rightBlind.transform.rotation = gameObject.transform.rotation;
        rightBlind.transform.Translate(1f, 0f, 0.999f);
        rightBlind.layer = PANORAMA_LAYER;

        Material mat = leftBlind.GetComponent<MeshRenderer>().material;
        mat.shader = Shader.Find("Standard");
        mat.SetColor("_Color", new Color(0, 0, 0, 0.75f));
        mat.SetFloat("_Mode", 2.0f);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;

        rightBlind.GetComponent<MeshRenderer>().sharedMaterial = mat;

        UpdateDebug();
    }
}
