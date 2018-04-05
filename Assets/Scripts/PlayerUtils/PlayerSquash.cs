using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSquash : MonoBehaviour
{
	private void OnTriggerEnter( Collider other )
	{
		if (other.tag!="Player") return;
		var p = other.GetComponent<Character> ();
		p.StartCoroutine (p.BlockPlayer (1f));
		p.anim.SetTrigger ("Squash");
	}
}
