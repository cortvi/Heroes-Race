using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientCamera : MonoBehaviour
{
	#region DATA
	public Vector3 offset;
	internal Character target;
	private Vector3 actualOffset;
	#endregion

	#region UTILS
	private void ComputeMotion () 
	{
		// Project position over XZ plane
		var forward = target.transform.position;
		forward.y = 0f;
		forward.Normalize ();

		// Compute the rotation matrix
		var mat = Matrix4x4.Rotate (Quaternion.LookRotation (forward));

		// Invert side-offset based on moving direction
		actualOffset = new Vector3 (actualOffset.x, offset.y, offset.z);
		actualOffset.x = Mathf.Lerp
		(
			actualOffset.x,
			(target.movingDir > 0f)? +offset.x : -offset.x,
			Time.deltaTime * 3f
		);

		// Get the position and add the offset
		var pos = target.transform.position;
		pos += mat.MultiplyVector (actualOffset);

		// Lerp for smooth position follow
		transform.position = Vector3.Lerp (transform.position, pos, Time.deltaTime * 7f);

		// Project camera position for its rotation
		var camForward = transform.position;
		camForward.y = 0f;
		transform.rotation = Quaternion.LookRotation (-camForward.normalized);
	}
	#endregion

	private void Update () 
	{
		ComputeMotion ();
	}
}
