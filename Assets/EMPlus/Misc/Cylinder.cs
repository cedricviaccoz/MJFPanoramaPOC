using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cylinder : MonoBehaviour {
	[ReadOnlyWhenPlaying] public bool insideOut = true;
	[ReadOnlyWhenPlaying] public int segments = 36;
	public Material material = null;
	public Color color = Color.white;
	public float lineWidth = 0.1f;

	// Use this for initialization
	void Start () {
		if (segments < 6) segments = 6;
		if (segments > 360) segments = 360;
		CylinderGenerator.Create(gameObject, insideOut, segments);
		var cylinderRenderer = gameObject.AddComponent<MeshRenderer>();
		if (material != null) {
			cylinderRenderer.sharedMaterial = material;
		} else {
			cylinderRenderer.material.shader = Shader.Find("EMPlus/GridShader");
			cylinderRenderer.material.SetColor("_Color", color);
			cylinderRenderer.material.SetFloat("_LineWidth", lineWidth);
		}
	}
}
