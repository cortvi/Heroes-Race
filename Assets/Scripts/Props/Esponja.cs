using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Esponja : MonoBehaviour
{
	public float throwForce;
	Animator anim;

	public void ThrowPlayers()
	{
		var players = Physics.OverlapSphere (transform.parent.position, 0.5f);
		foreach (var col in players)
		{
			if (col.tag!="Player") continue;
			var body = col.GetComponent<Rigidbody> ();
			var p = col.GetComponent<Player> ();
			if (p.cannotWork) continue;

			var jumpDir = transform.right * throwForce;
			body.AddForceAtPosition (jumpDir, p.anim.transform.position, ForceMode.VelocityChange);
			p.OnAir = true;
		}
		anim.ResetTrigger ("Charge");
	}

	private void OnCollisionEnter( Collision col )
	{
		if (col.gameObject.tag=="Player")
		{
			if (col.transform.position.y < transform.position.y) return;
			if (anim.GetCurrentAnimatorStateInfo (0).IsName("None")) anim.SetTrigger ("Charge");
		}
	}

	private void Awake()
	{
		anim = transform.parent.GetComponent<Animator> ();
	}
}
