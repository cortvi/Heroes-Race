using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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

		var p = t.GetComponent<Player> ();
		p.SetTrigger ("Hit");
		p.StartCoroutine (p.BlockPlayer (1.4f));
	}

	private void OnCollisionEnter ( Collision col )
	{
		if (col.gameObject.tag!="Player") return;
		if (!col.gameObject.GetComponent<Player> ().hasAuthority) return;
		Push (col.transform);
	}

	private void OnTriggerEnter( Collider other )
	{
		if (other.tag!="Player") return;
		if (!other.GetComponent<Player> ().hasAuthority) return;
		Push (other.transform);
	}
}
