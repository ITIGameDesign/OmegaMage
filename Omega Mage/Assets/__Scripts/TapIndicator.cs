using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/*
TapIndicator makes use of the PT_Mover class from ProtoTools. This allows it to
use a Bezier curve to alter position, rotation, scale, etc.
You'll also notice that this adds several public fields to the Inspector.
*/

public class TapIndicator : PT_Mover {

	public float lifeTime = 0.4f; // How long will it last
	public float[] scales; // The scales it interpolates
	public Color[] colors; // The colors it interpolates
	void Awake() {
		scale = Vector3.zero; // This initially hides the indicator
	}
	void Start () {
		// PT_Mover works based on the PT_Loc class, which contains information
		// about position, rotation, and scale. It's similar to a Transform but
		// simpler (and Unity won't let us create Transforms at will).
		PT_Loc pLoc;
		List<PT_Loc> locs = new List<PT_Loc>();
		// The position is always the same and always at z=-0.1f
		Vector3 tPos = pos;
		tPos.z = -0.1f;
		// You must have an equal number of scales and colors in the Inspector
		for (int i=0; i<scales.Length; i++) {
			pLoc = new PT_Loc();
			pLoc.scale = Vector3.one * scales[i]; // Each scale
			pLoc.pos = tPos;
			pLoc.color = colors[i]; // and each color
			locs.Add(pLoc); // is added to locs
		}
		// callback is a function delegate that can call a void function() when
		// the move is done
		callback = CallbackMethod; // Call CallbackMethod() when finished
		// Initiate the move by passing in a series of PT_Locs and duration for
		// the Bézier curve.
		PT_StartMove(locs, lifeTime);
	}
	void CallbackMethod() {
		Destroy(gameObject); // When the move is done, Destroy(gameObject)
	}

}