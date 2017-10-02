using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiranaVolarina : MonoBehaviour
{
	new BoxCollider collider;
	Transform child;

	private void Awake()
	{
		collider = GetComponent<BoxCollider> ();
		child = transform.GetChild (0);
	}

	private void LateUpdate()
	{
		collider.center = child.localPosition;
	}
}
