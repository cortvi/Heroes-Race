using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Esponja : MonoBehaviour
{
	public float throwForce;
	Animator anim;
	NetworkAnimator animN;

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

	private void OnCollisionStay ( Collision col ) 
	{
		if (col.gameObject.tag=="Player")
		{
			if (col.gameObject.GetComponent<PlayerOnline> ().cannotWork) return;
			if (Vector3.Distance (transform.parent.position, col.transform.GetChild(0).position)>0.35f) return;
			if (anim.GetCurrentAnimatorStateInfo (0).IsName("None")) animN.SetTrigger ("Charge");
		}
	}

	private void Awake()
	{
		anim = transform.parent.GetComponent<Animator> ();
		animN = transform.parent.GetComponent<NetworkAnimator> ();
	}
}
