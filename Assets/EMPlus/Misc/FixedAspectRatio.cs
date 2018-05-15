using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedAspectRatio : MonoBehaviour {
	public Vector2 aspectRatio = new Vector2(16f, 9f);
	private Camera targetCamera;

	// Use this for initialization
	void Start () 
	{
		float targetaspect = aspectRatio.x / aspectRatio.y;

		// determine the game window's current aspect ratio
		float windowaspect = (float)Screen.width / (float)Screen.height;

		// current viewport height should be scaled by this amount
		float scaleheight = windowaspect / targetaspect;

		// obtain camera component so we can modify its viewport
		targetCamera = GetComponent<Camera>();

		// if scaled height is less than current height, add letterbox
		if (scaleheight < 1.0f)
		{  
			Rect rect = targetCamera.rect;

			rect.width = 1.0f;
			rect.height = scaleheight;
			rect.x = 0;
			rect.y = (1.0f - scaleheight) / 2.0f;

			targetCamera.rect = rect;
		}
		else // add pillarbox
		{
			float scalewidth = 1.0f / scaleheight;

			Rect rect = targetCamera.rect;

			rect.width = scalewidth;
			rect.height = 1.0f;
			rect.x = (1.0f - scalewidth) / 2.0f;
			rect.y = 0;

			targetCamera.rect = rect;
		}
	}

	// Update is called once per frame
	void Update () {
		
	}
	/*
	void OnPreCull()
	{
		if (Application.isEditor) return;
		Rect wp = camera.rect;
		Rect nr = new Rect(0, 0, 1, 1);

		camera.rect = nr;
		GL.Clear(true, true, Color.black);

		camera.rect = wp;

	}
	*/
}
