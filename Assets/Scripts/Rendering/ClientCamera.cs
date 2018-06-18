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
		// Invert side-position based on moving direction
//		offset.x *= target.movDir > 0f ? +1f : -1f;
		// Transform offset based on the rotation of the target Character's driver
		var pos = target.driver.transform.TransformPoint (offset);
		// Lerp for smooth follow
		transform.position = Vector3.Lerp (transform.position, pos, Time.deltaTime * 7f);
		print (target.driver.name);
	}

	private void Rotate () 
	{
		#warning just a test
		transform.rotation = Quaternion.LookRotation (-target.driver.transform.forward);
	}
	#endregion

	private void Update () 
	{
		Move ();
		Rotate ();
	}
}
