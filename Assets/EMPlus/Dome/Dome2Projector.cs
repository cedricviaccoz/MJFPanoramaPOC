using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

[Serializable]
public class Dome2Projector : MonoBehaviour
{
    public DomeCameraRig cameraRig;
    private Camera originalCam;

    public float projectorFov = 60;
    public float projectorShiftX = 0;
    public float projectorShiftY = 0;
    public float blendWidth = 10;
    public float blendPower = 1;
    public float blendGamma = 1;
    public bool showGrid = false;

    public string configFilename = "domeConfig.txt";
    public bool saveConfig = false;
    public bool loadConfig = true;

    [NonSerialized] [HideInInspector] public GameObject sphereTransform;
    [NonSerialized] [HideInInspector] public GameObject sphere;
    private bool isInitialized = false;

    void Start () {
        originalCam = gameObject.GetComponent<Camera>();
        saveConfig = false;
        showGrid = false;

    }

    void Update () {
        if (!cameraRig)
        {
            cameraRig = gameObject.GetComponent<DomeCameraRig>();
        }
        if (!isInitialized && cameraRig && cameraRig.isInitialized)
        {
            SetupSphere();
            projectorFov = originalCam.fieldOfView;
            ConfigureOriginalCam();
            OnValidate();
            isInitialized = true;
        }
        if (saveConfig) {
            saveConfig = false;
            SaveJsonFile();
        }
        if (loadConfig) {
            loadConfig = false;
            LoadJsonFile();
        }
    }

    void OnValidate()
    {
        SetCameraMatrix();
        if (sphere != null)
        {
            var sphereRenderer = sphere.GetComponent<MeshRenderer>();
            if (showGrid)
                sphereRenderer.material.EnableKeyword("GRID_ON");
            else
                sphereRenderer.material.DisableKeyword("GRID_ON");
            sphereRenderer.material.SetFloat("_BlendWidth", blendWidth);
            sphereRenderer.material.SetFloat("_BlendPower", blendPower);
            sphereRenderer.material.SetFloat("_BlendGamma", blendGamma);
        }
    }

    void ConfigureOriginalCam()
    {
        originalCam.cullingMask = (1 << DomeCameraRig.DOME_LAYER);
        originalCam.clearFlags = CameraClearFlags.SolidColor;
        originalCam.backgroundColor = Color.black;
    }

    void SetupSphere()
    {
        sphereTransform = new GameObject("Sphere Transform");
        sphereTransform.layer = DomeCameraRig.DOME_LAYER;
        sphereTransform.transform.SetParent(transform);
        sphereTransform.transform.position = gameObject.transform.position;
        sphereTransform.transform.rotation = gameObject.transform.rotation;

        sphere = new GameObject("Dome Sphere");
        sphere.layer = DomeCameraRig.DOME_LAYER;
        IcoSphereGenerator.Create(sphere, true, 4);
        var sphereRenderer = sphere.AddComponent<MeshRenderer>();
        sphere.transform.SetParent(sphereTransform.transform);
        sphere.transform.position = sphereTransform.transform.position;
        sphere.transform.rotation = sphereTransform.transform.rotation;
        sphere.transform.Rotate(new Vector3(0, -135, 0));

        sphereRenderer.material.shader = Shader.Find("Unlit/Dome2ProjectorShader");
        sphereRenderer.material.SetTexture("_LeftTex",      cameraRig.renderTextures[0]);
        sphereRenderer.material.SetTexture("_RightTex",     cameraRig.renderTextures[1]);
        sphereRenderer.material.SetTexture("_TopTex",       cameraRig.renderTextures[2]);
        sphereRenderer.material.SetTexture("_BottomTex",    cameraRig.renderTextures[3]);

        sphereRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        sphereRenderer.receiveShadows = false;
        sphereRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        sphereRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

        sphere.hideFlags = HideFlags.HideAndDontSave;

        sphereTransform.transform.Translate(0f, 0f, 1f);
        sphereTransform.transform.Rotate(-90f, 0f, 0f);
    }

    void SetCameraMatrix()
    {
        if (originalCam == null)
            return;
        Debug.Log("setting camera fov/shift");
        originalCam.ResetProjectionMatrix();
        originalCam.fieldOfView = projectorFov;
        Matrix4x4 mat = originalCam.projectionMatrix;
        mat[0, 2] = projectorShiftX / 100f;
        mat[1, 2] = projectorShiftY / 100f;
        originalCam.projectionMatrix = mat;
    }

    /*** SERIALIZATION ***/
    [Serializable]
    public struct s_sphereAlignment
    {
        public Quaternion rotation;
        public Vector3 position;
    }
    [HideInInspector] public s_sphereAlignment sphereAlignment;
    private class SerializeDome
    {
        public float projectorFov = 60;
        public float projectorShiftX = 0;
        public float projectorShiftY = 0;
        public float blendWidth = 10;
        public float blendPower = 1;
        public float blendGamma = 1;
        public s_sphereAlignment sphereAlignment = new s_sphereAlignment();
    }
    public void LoadJsonFile()
    {
        if (!File.Exists(configFilename))
        {
            Debug.LogError("unable to load config file: " + configFilename);
            return;
        }
        Debug.Log("loading config file: " + configFilename);
        var o = JsonUtility.FromJson<SerializeDome>(File.ReadAllText(configFilename));
        projectorFov = o.projectorFov;
        projectorShiftX = o.projectorShiftX;
        projectorShiftY = o.projectorShiftY;
        blendWidth = o.blendWidth;
        blendPower = o.blendPower;
        blendGamma = o.blendGamma;
        if (sphereTransform != null)
        {
            sphereTransform.transform.position = gameObject.transform.position;
            sphereTransform.transform.rotation = gameObject.transform.rotation;
            sphereTransform.transform.Translate(o.sphereAlignment.position);
            sphereTransform.transform.Rotate(o.sphereAlignment.rotation.eulerAngles);
        }
        OnValidate();
    }
    public void SaveJsonFile()
    {
        Debug.Log("saving config file: " + configFilename);
        sphereAlignment.rotation = sphereTransform.transform.localRotation;
        sphereAlignment.position = sphereTransform.transform.localPosition;
        var jsonString = JsonUtility.ToJson(this, true);
        File.WriteAllText(configFilename, jsonString);
    }

}
