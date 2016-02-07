using UnityEngine;
using System.Collections;

public class ElementInventoryButton : MonoBehaviour {

	public ElementType type;
	void Awake() {
		// Parse the first character of the name of this GameObject into an int
		char c = gameObject.name[0];
		string s = c.ToString();
		int typeNum = int.Parse(s);
		// typecast that int to an ElementType
		type = (ElementType) typeNum;
	}
	void OnMouseUpAsButton() {
		// Tell the Mage to add this element type
		Mage.S.SelectElement(type);
	}
}
