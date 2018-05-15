using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualPanoramaScreen : MonoBehaviour {
	public PanoramaCameraRig cameraRig;
	public float diameter = 5;
	public float screenHeight = 4;
	public float aboveFloor = 0.2f;
	[ReadOnlyWhenPlaying] public bool undistortSlices = true;
	public Vector3 viewerPosition = new Vector3(0f, 1.65f, 0f);
	[HideInInspector] public GameObject cylinder = null;
	[HideInInspector] public GameObject cylinderBlack = null;
	[HideInInspector] public GameObject floor = null;
    MeshRenderer cylinderRenderer = null;

    private bool isInitialized = false;

	void SetupCylinder() {
		cylinder = new GameObject("Virtual Panorama Cylinder");
//		cylinder.transform.SetParent(transform);
		cylinder.transform.position = new Vector3(0f, aboveFloor + screenHeight*0.5f, 0f) - viewerPosition;
		float sr = diameter / 2f;
		float sh = screenHeight / 2f;
		cylinder.transform.localScale = new Vector3 (sr, sh, sr);
		cylinder.layer = PanoramaCameraRig.PANORAMA_LAYER;
		CylinderGenerator.Create(cylinder, true, 180);
		cylinderRenderer = cylinder.AddComponent<MeshRenderer>();
		if (undistortSlices) {
			cylinderRenderer.material = new Material (Shader.Find ("EMPlus/PanoramaSliceWarpShader"));
			cylinderRenderer.material.SetVector ("_SliceLayout", new Vector4 (cameraRig.numSlices, cameraRig.sliceAngle * Mathf.Deg2Rad, 0f, 0f));
		} else {
			cylinderRenderer.material = new Material (Shader.Find ("Unlit/Texture"));
		}
		cylinderRenderer.material.SetTexture ("_MainTex", cameraRig.renderTexture);
		cylinderRenderer.material.SetTextureScale("_MainTex", new Vector2(360f / cameraRig.RenderedAngle(), 1f));

		cylinderBlack = new GameObject ("Virtual Cylinder Outside Black");
		CylinderGenerator.Create(cylinderBlack, true, 90);
		cylinderBlack.AddComponent<MeshRenderer>();
		cylinderBlack.layer = PanoramaCameraRig.PANORAMA_LAYER;
		cylinderBlack.transform.SetParent (cylinder.transform);
		Material matBlack = cylinderBlack.GetComponent<MeshRenderer>().material;
		matBlack.shader = Shader.Find("Unlit/Color");
		matBlack.SetColor("_Color", new Color(0, 0, 0, 1f));

		floor = GameObject.CreatePrimitive (PrimitiveType.Quad);
		floor.name = "Virtual Panorama Screen Floor Quad";
		floor.layer = PanoramaCameraRig.PANORAMA_LAYER;
		floor.transform.SetParent (cylinder.transform);
		floor.transform.rotation = Quaternion.Euler (90f, 0f, 0f);
		Material matFloor = floor.GetComponent<MeshRenderer>().material;
		matFloor.shader = Shader.Find("Standard");
		matFloor.SetColor("_Color", new Color(0.1f, 0.1f, 0.1f, 1f));

		OnValidate ();
	}

	void Update () {
		if (!cameraRig)
		{
			cameraRig = gameObject.GetComponent<PanoramaCameraRig>();
		} else
        {
            if (cylinderRenderer != null)
            {
                cylinderRenderer.material.SetFloat("_ShiftY", cameraRig.cameraShiftY / 200f);
            }
        }
        if (!isInitialized && cameraRig && cameraRig.isInitialized)
		{
			SetupCylinder();
			isInitialized = true;
		}
	}

	void OnValidate()
	{
		float sr = diameter / 2f;
		float sh = screenHeight / 2f;
        if (cylinder != null)
        {
            cylinder.transform.position = new Vector3(0f, aboveFloor + screenHeight * 0.5f, 0f) - viewerPosition;
            cylinder.transform.localScale = new Vector3(sr, sh, sr);
        }
        if (cylinderBlack != null)
        {
            cylinderBlack.transform.position = cylinder.transform.position - new Vector3(0f, aboveFloor * 0.5f, 0f);
            cylinderBlack.transform.localScale = new Vector3(1.01f, (screenHeight + aboveFloor) / screenHeight, 1.01f);
        }
        if (floor != null)
        {
            floor.transform.localScale = new Vector3(2f, 2f, 1f);
            floor.transform.position = new Vector3(0f, -viewerPosition.y, 0f);
        }
    }
}
