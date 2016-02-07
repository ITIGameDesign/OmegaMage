using UnityEngine;
using System.Collections;

public interface Enemy {
	// These are declarations of properties that will be implemented by all
	// Classes that implement the Enemy interface
	Vector3 pos { get; set; } // The Enemy's transform.position
	float touchDamage { get; set; } // Damage done by touching the Enemy
	string typeString { get; set; } // The type string from Rooms.xml

	// The following are already implemented by all MonoBehaviour subclasses
	GameObject gameObject { get; }
	Transform transform { get; }
}
