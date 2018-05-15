/***
 * This Script will create a Quad to display the Fisheye texture from the four RenderTextures
 * of a DomeCameraRig that is either connected to the same GameObject or given by cameraRig
 ***/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DomeFisheye : MonoBehaviour {
    public DomeCameraRig cameraRig;
    private GameObject plane;
    private Camera originalCam;
    private bool isInitialized = false;

    void Start () {
        originalCam = gameObject.GetComponent<Camera>();
        plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        plane.name = "Dome Fisheye Display Quad";
        plane.transform.SetParent(gameObject.transform);
        plane.transform.position = gameObject.transform.position;
        plane.transform.rotation = gameObject.transform.rotation;
        plane.transform.Translate(0f, 0f, 1f);
        plane.layer = DomeCameraRig.DOME_LAYER;

        plane.AddComponent<DomeFisheyeTexturer>();
    }

    void Update()
    {
        if (!cameraRig)
        {
            cameraRig = gameObject.GetComponent<DomeCameraRig>();
        }
        if (!isInitialized && cameraRig && cameraRig.isInitialized)
        {
            ConfigureOriginalCam();
            isInitialized = true;
        }
    }
        void ConfigureOriginalCam()
    {
        Debug.Log("DomeFisheye: configuring original camera as orthographic");
        originalCam.cullingMask = (1 << DomeCameraRig.DOME_LAYER);
        originalCam.clearFlags = CameraClearFlags.SolidColor;
        originalCam.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
        originalCam.orthographic = true;
        originalCam.orthographicSize = 0.5f;
    }
}
