using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelChange : MonoBehaviour
{
	public bool down;

	private void OnTriggerEnter ( Collider other )
	{
		if (other.tag!="Player") return;
		var p = other.GetComponent<Player> ();
		p.cam.SetTrigger (down? "GoDown" : "GoUp");
	}
}
