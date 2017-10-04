using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSquash : MonoBehaviour
{
	private void OnTriggerEnter( Collider other )
	{
		if (other.tag!="Player") return;
		var p = other.GetComponent<PlayerOnline> ();
		p.StartCoroutine (p.BlockPlayer (1f));
		p.animN.SetTrigger ("Squash");
	}
}
