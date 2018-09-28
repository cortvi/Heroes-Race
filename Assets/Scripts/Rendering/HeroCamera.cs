using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroesRace 
{
	public class HeroCamera : MonoBehaviour 
	{
		#region DATA
		public int floor;
		public Vector3 offset = new Vector3 (2.78f, 2.93f, 9.63f);

		internal Hero target;
		private Vector3 actualOffset;

		private const float FloorHeigth = 5.2f;
		#endregion

		private void Update () 
		{
			if (!target)
			{
				Destroy (this);
				return;
			}

			// Project position over XZ plane
			var forward = target.transform.position;
			forward.y = 0f;
			forward.Normalize ();

			// Compute the rotation matrix
			var mat = Matrix4x4.Rotate (Quaternion.LookRotation (forward));

			// Lerp side-offset based on moving direction more smoothly
			actualOffset = new Vector3 (actualOffset.x, offset.y, offset.z);
			actualOffset.x = Mathf.Lerp
			(
				actualOffset.x,
				(target.movingDir > 0f) ? +offset.x : -offset.x,
				Time.deltaTime * 2f
			);
			
			// Get the final position (+floor height)
			var pos = target.transform.position;
			pos.y = floor * FloorHeigth;
			pos += mat.MultiplyVector (actualOffset);

			// Lerp the position for a smooth camera follow
			transform.position = Vector3.Lerp (transform.position, pos, Time.deltaTime * 7f);

			// Project camera position to get the orientation
			var camForward = transform.position;
			camForward.y = 0f;
			transform.rotation = Quaternion.LookRotation (-camForward.normalized);
		}
	} 
}
