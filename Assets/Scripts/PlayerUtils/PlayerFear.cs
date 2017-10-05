using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFear : MonoBehaviour
{
	private void OnTriggerEnter( Collider other )
	{
		if (other.tag!="Player") return;
		var p = other.GetComponent<Player> ();
		if (!p.hasAuthority) return;

		p.StartCoroutine (p.BlockPlayer (1.8f));
		p.SetTrigger ("Fear");
	}
}
