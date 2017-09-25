using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushWall : MonoBehaviour
{
	Animation anim;

	private void OnTriggerEnter( Collider other )
	{
		if (other.tag!="Player") return;
		anim.Play ();
	}

	private void Awake() 
	{
		anim = transform.GetChild (0).GetComponent<Animation> (); 
	}
}
