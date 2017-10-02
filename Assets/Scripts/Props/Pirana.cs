using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pirana : MonoBehaviour
{
	public float sleepTime;
	new BoxCollider collider;
	Rigidbody body;
	Vector3 ogPos;
	Quaternion ogRot;
	
	IEnumerator Work ()
	{
		while (true)
		{
			body.useGravity = true;
			transform.localPosition = ogPos;
			transform.localRotation = ogRot;
			body.AddForce (Vector3.up * 8.9f, ForceMode.Impulse);
			yield return new WaitForSeconds (0.2f);
			body.AddRelativeTorque (Vector3.right * 2.4f, ForceMode.Impulse);
			yield return new WaitForSeconds (1.615f);
			body.velocity = Vector3.zero;
			body.angularVelocity = Vector3.zero;
			body.useGravity = false;
			yield return new WaitForSeconds (sleepTime);
		}
	}

	private void Start () 
	{
		body = GetComponent<Rigidbody> ();
		collider = transform.parent.GetComponent<BoxCollider> ();
		ogPos = transform.localPosition;
		ogRot = transform.localRotation;
		StartCoroutine ("Work");
	}

	private void LateUpdate() 
	{
		var newPos = collider.center;
		newPos.y = transform.localPosition.y;
		collider.center = newPos;
	}
}
