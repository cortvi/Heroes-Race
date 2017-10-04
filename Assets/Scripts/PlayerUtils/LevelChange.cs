using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelChange : MonoBehaviour
{
	public bool down;

	private void OnTriggerEnter ( Collider other )
	{
		if (other.tag!="Player") return;
		var p = other.GetComponent<PlayerOnline> ();
		p.camN.SetTrigger (down? "GoDown" : "GoUp");
	}
}
