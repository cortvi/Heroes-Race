using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSquash : MonoBehaviour
{
	private void OnTriggerEnter( Collider other )
	{
		if (other.tag!="Player") return;
		var p = other.GetComponent<Player> ();
		if (!p.hasAuthority) return;

		p.StartCoroutine (p.BlockPlayer (1f));
		p.SetTrigger ("Squash");
	}
}
