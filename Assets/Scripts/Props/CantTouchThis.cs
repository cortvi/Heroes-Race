using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CantTouchThis : MonoBehaviour
{
	public float force;

	private void OnCollisionEnter( Collision col )
	{
		if (col.gameObject.tag!="Player") return;

		var dist = transform.InverseTransformPoint (col.transform.GetChild (0).position);
		var factor = Mathf.Sign (dist.x);
		var vForce = (( transform.right * factor ) + Vector3.up*0.6f) * force;
		var r = col.gameObject.GetComponent<Rigidbody> ();
		r.velocity = Vector3.zero;
		r.AddForceAtPosition (vForce, transform.position, ForceMode.VelocityChange);

		var p = col.gameObject.GetComponent<Player> ();
		p.StartCoroutine (p.BlockPlayer (1.4f));
		p.anim.SetTrigger ("Hit");
	}
}
