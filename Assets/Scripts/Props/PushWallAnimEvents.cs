using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushWallAnimEvents : MonoBehaviour
{
	private void OnTriggerEnter( Collider other )
	{
		if (other.tag!="Player") return;
		var p = other.GetComponent<Player> ();
		p.anim.SetTrigger ("Push");
		p.anim.SetTrigger ("Hit");
		p.StartCoroutine (p.BlockPlayer (1.1f, true));
	}
}
