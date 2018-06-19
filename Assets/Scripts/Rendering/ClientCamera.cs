using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientCamera : MonoBehaviour
{
	#region DATA
	public int floor;
	public Vector3 offset = new Vector3 (2.78f, 2.93f, 9.63f);

	internal Character target;
	private Vector3 actualOffset;

	private const float FloorHeigth = 5.2f;
	#endregion

	private void Update ()
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
			(target.movingDir > 0f) ? +offset.x : -offset.x,
			Time.deltaTime * 3f
		);

		// Get the position and add the offset
		var pos = target.transform.position;
		pos.y = floor * FloorHeigth;
		pos += mat.MultiplyVector (actualOffset);

		// Lerp for smooth position follow
		transform.position = Vector3.Lerp (transform.position, pos, Time.deltaTime * 7f);

		// Project camera position for its rotation
		var camForward = transform.position;
		camForward.y = 0f;
		transform.rotation = Quaternion.LookRotation (-camForward.normalized);
	}
}
