using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cam : MonoBehaviour
{
	public float y;

	private void LateUpdate()
	{
		transform.position = new Vector3 (transform.position.x, y, transform.position.z);
	}

	private void Awake()
	{
		y = transform.position.y;
	}
}