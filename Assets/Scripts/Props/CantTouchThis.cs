using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CantTouchThis : MonoBehaviour
{
	public float force;
	public bool upInstead;

	void Push ( Transform t ) 
	{
		var dist = t.GetChild(0).InverseTransformPoint (transform.position);
		var dir = Mathf.Sign (-dist.z);
		var v = upInstead ? t.GetChild (0).up : t.GetChild (0).forward;
		var vForce = ((v * dir) + Vector3.up*0.6f) * force;
		var r = t.GetComponent<Rigidbody> ();
		r.velocity = Vector3.zero;
		r.angularVelocity = Vector3.zero;
		r.AddForceAtPosition (vForce, transform.position, ForceMode.VelocityChange);

		var p = t.GetComponent<Character> ();
		p.anim.SetTrigger ("Hit");
		p.StartCoroutine (p.BlockPlayer (1.4f));
	}

	private void OnCollisionEnter ( Collision col )
	{
		if (col.gameObject.tag!="Player") return;
		Push (col.transform);
	}

	private void OnTriggerEnter( Collider other )
	{
		if (other.tag!="Player") return;
		Push (other.transform);
	}
}
