using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ascensor : MonoBehaviour
{
	public bool broken;
	public Rigidbody[] parts;

	new BoxCollider collider;
	Transform child;
	Animator anim;
	int playersIn;
	
	bool GoingUp 
	{
		get { return anim.GetBool ("GoingUp"); }
		set { anim.SetBool ("GoingUp", value); }
	}

	private void Update()  
	{
		UpdateCollider ();
		GoingUp = playersIn>0;
	}
	void UpdateCollider()  
	{
		var newZ = Vector3.forward * ( child.transform.localPosition.z + 0.11f);
		collider.center = newZ;
	}
	public void Break ()   
	{
		if (!broken) return;
		anim.enabled = false;
		foreach (var c in parts)
		{
			c.isKinematic = false;
			Destroy (c.gameObject, 2f);
		}
	} 

	private void OnTriggerEnter( Collider other ) 
	{
		if (other.tag!="Player") return;
		other.transform.SetParent (child);
		playersIn++;
	}
	private void OnTriggerExit( Collider other )  
	{
		if (other.tag!="Player") return;
		other.transform.SetParent (null);
		playersIn--;
	}

	private void Awake() 
	{
		child = transform.GetChild (0);
		anim = GetComponent<Animator> ();
		collider = GetComponent<BoxCollider> ();
	}
}
