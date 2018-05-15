using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CERN360Demo : MonoBehaviour {
    public CERN archive = null;
    public float fadeStep = 0.1f;
    private float gazeStep = 0.01f;
    public Transform referencePoint;
    public float scale = 1f;
    [ReadOnlyWhenPlaying] public bool isCylinder = false;
    [ReadOnlyWhenPlaying] public bool useDxtFiles = false;
    public float shiftU = 0.0f;
    public float scaleCylinderV = 0.4f;

    private int recordIndex = 0;
    private int imageIndex = 0;
    private float updateFade = 0;
    private float gazeTrigger = 0;
    private MeshRenderer meshRenderer = null;
    private GameObject infoBlock;
    public Shader sphereShader;
    public Shader cylinderShader;

    void Start ()
    {
        if (isCylinder)
            CylinderGenerator.Create(gameObject, true, 64);
        else
            IcoSphereGenerator.Create(gameObject, true, 5);

        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        if (isCylinder)
        {
            meshRenderer.material.shader = cylinderShader;
            meshRenderer.material.EnableKeyword("FROM_SPHERICAL");
            meshRenderer.material.SetFloat("_Fade", 1f);
            meshRenderer.material.SetFloat("_ShiftU", shiftU);
            meshRenderer.material.SetFloat("_ScaleV", scaleCylinderV);
        } else
        {
            meshRenderer.material.shader = sphereShader;
            meshRenderer.material.SetFloat("_Fade", 1f);
            meshRenderer.material.SetFloat("_ShiftU", shiftU);
        }

        infoBlock = new GameObject(name + " Info");
        infoBlock.transform.SetParent(transform);
        infoBlock.transform.localPosition = new Vector3(0f, -0.5f, 0.5f);
        infoBlock.transform.localRotation = Quaternion.Euler(45f, 0f, 0f);

        var infoText = infoBlock.AddComponent<TextMesh>();
        infoText.text = "Record: Not yet loaded\nImage: N/A";
        infoText.anchor = TextAnchor.MiddleCenter;
        infoText.fontSize = 40;
        infoText.characterSize = 0.005f;

        OnValidate();

        if (Application.sandboxType != ApplicationSandboxType.NotSandboxed)
            archive.jsonPath = Application.streamingAssetsPath;
        archive.Load();
        ChangeRecord(-1);
    }

    void UpdateImage()
    {
        string path = archive.jsonPath + "/Photos/" + archive.images[archive.records.items[recordIndex].files[imageIndex]].localFile;
        if (path.EndsWith(".tif"))
            path = path + ".jpg";

        Debug.Log("loading new image: " + path);
        Texture2D Tex2D;
        byte[] FileData;
        if (File.Exists(path))
        {
#if (!UNITY_WSA) || UNITY_EDITOR
            FileData = File.ReadAllBytes(path);
#else
            FileData = UnityEngine.Windows.File.ReadAllBytes(path);
#endif
            Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
            if (Tex2D.LoadImage(FileData))
            {
                infoBlock.GetComponent<TextMesh>().text = "Record " + (recordIndex + 1) + "/" + archive.records.items.Length + ": " + archive.records.items[recordIndex].title + "\nImage " + (imageIndex + 1) + "/" + archive.records.items[recordIndex].files.Length + ": " + archive.images[archive.records.items[recordIndex].files[imageIndex]].full_name;
                gameObject.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", Tex2D);
            }
            else
                infoBlock.GetComponent<TextMesh>().text = "Record " + (recordIndex + 1) + "/" + archive.records.items.Length + ": " + archive.records.items[recordIndex].title + "\nImage " + (imageIndex + 1) + "/" + archive.records.items[recordIndex].files.Length + ": [Could not be loaded]";
        }
        else
        {
            infoBlock.GetComponent<TextMesh>().text = "Record " + (recordIndex + 1) + "/" + archive.records.items.Length + ": " + archive.records.items[recordIndex].title + "\nImage " + (imageIndex + 1) + "/" + archive.records.items[recordIndex].files.Length + ": [File not found]";
            Debug.Log("No such image file: " + path);
        }
    }

    void UpdateImageDXT1()
    {
        string path = archive.jsonPath + "/DXT1_8192x4096/" + archive.images[archive.records.items[recordIndex].files[imageIndex]].localFile + ".dxt";
        Debug.Log("loading new compressed image: " + path);

        Texture2D Tex2D = LoadTextureDXT.Load(path, true);
        if (Tex2D != null)
        {
            infoBlock.GetComponent<TextMesh>().text = "Record " + (recordIndex + 1) + "/" + archive.records.items.Length + ": " + archive.records.items[recordIndex].title + "\nImage " + (imageIndex + 1) + "/" + archive.records.items[recordIndex].files.Length + ": " + archive.images[archive.records.items[recordIndex].files[imageIndex]].full_name;
            gameObject.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", Tex2D);
        }
        else
            infoBlock.GetComponent<TextMesh>().text = "Record " + (recordIndex + 1) + "/" + archive.records.items.Length + ": " + archive.records.items[recordIndex].title + "\nImage " + (imageIndex + 1) + "/" + archive.records.items[recordIndex].files.Length + ": [Could not be loaded]";
    }

    void ChangeRecord(int dir)
    {
        if (archive.records.records.Length < 2) return;
        bool found = false;
        while (!found)
        {
            recordIndex+=dir;
            if (recordIndex < 0) recordIndex = archive.records.records.Length - 1;
            if (recordIndex >= archive.records.records.Length)
                recordIndex = 0;
            if (archive.records.items[recordIndex].files.Length > 0)
                found = true;
        }
        imageIndex = 0;
        Debug.Log("selected new record: " + archive.records.items[recordIndex].id);
        updateFade = fadeStep;
    }
    void ChangeImage(int dir)
    {
        int len = archive.records.items[recordIndex].files.Length;
        if (len < 2) return;
        imageIndex += dir;
        if (imageIndex < 0) imageIndex = len - 1;
        if (imageIndex >= len)
            imageIndex = 0;
        Debug.Log("selected new image: " + archive.records.items[recordIndex].files[imageIndex]);
        updateFade = fadeStep;
    }
    void NextImage()
    {
        int len = archive.records.items[recordIndex].files.Length;
        if (imageIndex == len - 1)
            ChangeRecord(1);
        else
            ChangeImage(1);
    }
    void Update()
    {
        float f = 1;
        if (updateFade > 0)
        {
            float newFade = updateFade + fadeStep;
            if (newFade >= 1 && updateFade <= 1)
            {
                if (useDxtFiles)
                    UpdateImageDXT1();
                else
                    UpdateImage();
                gameObject.transform.position = referencePoint.position;
                gameObject.transform.localRotation = Quaternion.Euler(0f, referencePoint.rotation.eulerAngles.y, 0f);
            }
            if (newFade > 2f)
            {
                gazeTrigger = -gazeStep;
                updateFade = 0;
            }
            else
                updateFade = newFade;
            f = Mathf.Clamp01(Mathf.Abs(1f - updateFade) * 1.2f - 0.2f);
        }
        else
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow))
                ChangeRecord(-1);
            else if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow))
                ChangeRecord(1);
            else if (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow))
                ChangeImage(-1);
            else if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow))
                ChangeImage(1);

            if (referencePoint.rotation.eulerAngles.x > 70f && referencePoint.rotation.eulerAngles.x < 110f)
            {
                if (gazeTrigger >= 0)
                {
                    gazeTrigger += gazeStep;
                    if (gazeTrigger > 1f)
                    {
                        gazeTrigger = 1f;
                        NextImage();
                    }
                }
            } else
                gazeTrigger = 0;
        }
        meshRenderer.material.SetFloat("_Fade", f * (1f - gazeTrigger));
        infoBlock.GetComponent<TextMesh>().color = new Color(1, 1f - gazeTrigger, 1f - gazeTrigger, f);
    }
    private void OnValidate()
    {
        transform.localScale = new Vector3(scale, scale, scale);
        if (infoBlock)
            infoBlock.transform.localScale = new Vector3(1,1,1);

        if (meshRenderer)
        {
            if (isCylinder)
            {
                meshRenderer.material.SetFloat("_Fade", 1f);
                meshRenderer.material.SetFloat("_ShiftU", shiftU);
                meshRenderer.material.SetFloat("_ScaleV", scaleCylinderV);
            }
            else
            {
                meshRenderer.material.SetFloat("_Fade", 1f);
                meshRenderer.material.SetFloat("_ShiftU", shiftU);
            }
        }
    }
}
