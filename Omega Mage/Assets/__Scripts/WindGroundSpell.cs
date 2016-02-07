using UnityEngine;
using System.Collections;

//Extends PT_Monobehaviour
public class WindGroundSpell : PT_MonoBehaviour
{
	public float duration = 3;  //Lifetime of this GameObject
	public float durationVariance = 0.5f;
	// ^ This allows the duration to range from 2.5 to 3.5
	public float fadeTime = 0.5f;  //Length of time to fade
	public float timeStart;  //Birth time of this GameObject
	public float damagePerSecond = 15;
	
	//Use this for initialization
	void Start ()
	{
		timeStart = Time.time;
		duration = Random.Range (duration - durationVariance, duration + durationVariance);
		//^Set the duration to a number between 3.5 and 4.5 (defaults)
	}
	
	//Update is called once per frame
	void Update ()
	{
		//Determine a number [0..1] (between 0 and 1) that stores the percentage of 
		//duration that has passed
		float u = (Time.time-timeStart)/duration;
		
		//At what u value should this start fading
		float fadePercent = 1 -(fadeTime/duration);
		if (u>fadePercent)  //IF it's after the time to start fading...
		{
			//...then sink into the ground
			float u2 = (u-fadePercent)/(1-fadePercent);
			//^u2 is a number [0..1] for just the fadeTime
			Vector3 loc = pos;
			loc.z = u2*2;  //move lower over time
			pos = loc;
		}
		
		if (u>1)
		{
			Destroy(gameObject);  //...destroy it
		}
	}
	
	void OnTriggerEnter(Collider other)
	{
		//Announce when another object enters the collider
		GameObject go = Utils.FindTaggedParent(other.gameObject);
		if (go == null)
		{
			go = other.gameObject;
		}
		Utils.tr("Wind hit",go.name);
	}
	
	void OnTriggerStay(Collider other)
	{
		//Actually damage the other
		//Get a refrence to the EnemyBug script component of the other
		EnemyBug recipient = other.GetComponent<EnemyBug> ();
		//If there is an enemyBug component, dmage it with air
		if (recipient != null)
		{
			recipient.Damage(damagePerSecond, ElementType.air, true);
		}
	}

	//TODO: Actually damage the other object
}