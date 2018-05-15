/***
 * This Script will create an IcoSphere with either normals pointing in or out
 * with the given number of subdivisions (1 < n < 7).
 * If no material is given, a Shader for Longitude/Latitude grid will be used
 * color and lineWidth parameters will be passed through to LongLatShader if used
 ***/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IcoSphere : MonoBehaviour {
    [ReadOnlyWhenPlaying] public bool insideOut = true;
    [ReadOnlyWhenPlaying] public int subdivisions = 3;
    public Material material = null;
    public Color color = Color.white;
    public float lineWidth = 0.1f;

    // Use this for initialization
    void Start () {
        if (subdivisions < 1) subdivisions = 1;
        if (subdivisions > 6) subdivisions = 6;
        IcoSphereGenerator.Create(gameObject, insideOut, subdivisions);
        var sphereRenderer = gameObject.AddComponent<MeshRenderer>();
        if (material != null) {
            sphereRenderer.sharedMaterial = material;
        } else {
            sphereRenderer.material.shader = Shader.Find("EMPlus/LongLatShader");
            sphereRenderer.material.SetColor("_Color", color);
            sphereRenderer.material.SetFloat("_LineWidth", lineWidth);
        }
    }
}
