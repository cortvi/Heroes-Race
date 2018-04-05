using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeny : MonoBehaviour
{
	public bool denyWork;
	public bool denyJump;

	private void OnTriggerEnter( Collider other ) 
	{
		if (other.tag!="Player") return;
		var p = other.GetComponent<Character> ();
		if (denyWork) p.cannotWork = true;
		if (denyJump) p.cannotJump = true;
	}

	private void OnTriggerExit( Collider other ) 
	{
		if (other.tag!="Player") return;
		var p = other.GetComponent<Character> ();
		if (denyWork) p.cannotWork = false;
		if (denyJump) p.cannotJump = false;
	}
}
