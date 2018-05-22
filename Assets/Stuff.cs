using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stuff : MonoBehaviour {
	public GameObject cylinder;
	float angle;

	// Use this for initialization
	void Start () {
		angle = 0;
	}
	
	// Update is called once per frame
	void Update () {
		if (UnityEngine.Input.GetKeyDown (KeyCode.Space)) {
			angle += 45;
			cylinder.transform.localRotation = Quaternion.Euler (0, angle, 0);
		}
	}
}
