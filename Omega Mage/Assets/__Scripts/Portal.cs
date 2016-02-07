using UnityEngine;
using System.Collections;

public class Portal : PT_MonoBehaviour
{
	public string toRoom;
	public bool justArrived = false;
	//^true if _Mage has just teleported here
	
	void OnTriggerEnter(Collider other)
	{
		if (justArrived) return;
		//^Since the mage has just arrived, don't teleport her back
		
		//Get the GameObject of the collider
		GameObject go = other.gameObject;
		//Search up for a tagged parent
		GameObject goP = Utils.FindTaggedParent(go);
		if (goP != null) go = goP;
		
		//If this isn't the _Mage, return
		if (go.tag != "Mage") return;
		//Go ahead and build the next room
		LayoutTiles.S.BuildRoom(toRoom);
	}
	
	void OnTriggerExit(Collider other)
	{
		//Once the Mage leaves this Portal, set justArrived to false
		if (other.gameObject.tag == "Mage")
		{
			justArrived = false;
		}
	}
}