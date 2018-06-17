using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientCamera : MonoBehaviour
{
	#region DATA
	public Vector3 offset;

	internal Character target;
	#endregion

	#region UTILS
	private void Move () 
	{
		// Transform offset based on the rotation of the target Character's driver
		var pos = target.driver.transform.TransformPoint (offset);
		// Invert side-position based on moving direction
		pos.x *= target.movDir > 0f ? +1f : -1f;
		// Lerp for smooth follow
		transform.position = Vector3.Lerp (transform.position, pos, Time.deltaTime * 7f);
	}

	private void Rotate () 
	{
		#warning just a test
		transform.LookAt (target.transform);
	}
	#endregion

	private void Update () 
	{
		Move ();
		Rotate ();
	}
}
