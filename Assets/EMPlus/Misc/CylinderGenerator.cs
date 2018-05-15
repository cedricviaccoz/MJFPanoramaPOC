using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CylinderGenerator
{
	public static void Create(GameObject go, bool flipTriangles, int segments = 32)
	{
		MeshFilter filter = go.AddComponent< MeshFilter >();
		Mesh mesh = filter.mesh;
		mesh.Clear();

		List<Vector3> vertList = new List<Vector3> ();
		List<Vector3> normList = new List<Vector3> ();
		List<Vector2> uvList = new List<Vector2> ();
		List<int> triList = new List<int> ();

		for (int s = 0; s <= segments; s++) {
			float p = (float)s / (float)segments;
			float a = p * Mathf.PI * 2f;
			vertList.Add(new Vector3(Mathf.Sin(a), -1f,  Mathf.Cos(a)));
			vertList.Add(new Vector3(Mathf.Sin(a),  1f,  Mathf.Cos(a)));
			uvList.Add(new Vector2(p, 0));
			uvList.Add(new Vector2(p, 1));
			if (flipTriangles) {
				normList.Add (new Vector3 (Mathf.Sin (a), 0f, Mathf.Cos (a)));
				normList.Add (new Vector3 (Mathf.Sin (a), 0f, Mathf.Cos (a)));
			} else {
				normList.Add (new Vector3 (Mathf.Sin (a) * -1f, 0f, Mathf.Cos (a) * -1f));
				normList.Add (new Vector3 (Mathf.Sin (a) * -1f, 0f, Mathf.Cos (a) * -1f));
			}
			if (s < segments) {
				triList.Add ((s * 2)     % (segments * 2 + 2));
				triList.Add ((s * 2 + 1) % (segments * 2 + 2));
				triList.Add ((s * 2 + 2) % (segments * 2 + 2));
				triList.Add ((s * 2 + 2) % (segments * 2 + 2));
				triList.Add ((s * 2 + 1) % (segments * 2 + 2));
				triList.Add ((s * 2 + 3) % (segments * 2 + 2));
			}
		}
		mesh.vertices = vertList.ToArray ();
		mesh.triangles = triList.ToArray ();
		mesh.normals = normList.ToArray ();
		mesh.uv = uvList.ToArray ();
	}
}
