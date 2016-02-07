using UnityEngine;
using System.Collections;

public class CameraFollow : PT_MonoBehaviour {

	public static CameraFollow S;

	public Transform targetTransform;
	public float camEasing = 0.1f;
	public Vector3 followOffset = new Vector3 (0, 0, -2);

	void Awake()
	{
		S = this;
	}

	void FixedUpdate()
	{
		Vector3 pos1 = targetTransform.position + followOffset;
		pos = Vector3.Lerp (pos, pos1, camEasing);
	}

}
