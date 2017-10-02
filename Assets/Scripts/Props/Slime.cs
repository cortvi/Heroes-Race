using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : MonoBehaviour
{
	float speed;
	Transform child;

	private void LateUpdate () 
	{
		if (!child) child=transform.GetChild (0);

		var rot = -speed * 120f * Time.deltaTime;
		transform.Rotate (Vector3.up, rot, Space.Self);
	}

	public void SwitchDir ( float dir ) 
	{ speed = dir; }
}
