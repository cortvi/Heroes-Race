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
		// Project position over XZ plane
		var forward = target.transform.position;
		forward.y = 0f;
		forward.Normalize ();

		// Compute the rotation matrix
		var mat = Matrix4x4.Rotate (Quaternion.LookRotation (forward));

		// Get the position and add the offset
		var pos = target.transform.position;
		pos += mat.MultiplyVector (offset);

		// Lerp for smooth follow
		transform.position = Vector3.Lerp (transform.position, pos, Time.deltaTime * 7f);
	}

	private void Rotate () 
	{
		#warning just a test
//		transform.rotation = Quaternion.LookRotation (target);
	}
	#endregion

	private void Update () 
	{
		Move ();
		Rotate ();
	}
}
