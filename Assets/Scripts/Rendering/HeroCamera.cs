using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class HeroCamera : MonoBehaviour 
	{
		#region DATA
		[Info] public Hero target;
		[Info] public int floor;

		private Vector3 actualOffset;
		public readonly Vector3 offset = new Vector3 (2.78f, 2.93f, 9.63f);
		private const float FloorHeigth = 5.2f;
		#endregion

		public IEnumerator SwitchLevel (int delta) 
		{
			float iOffset = actualOffset.y;
			float step = 0f;
			floor += delta;

			float duration = 1.5f;
			while (step <= 1f) 
			{
				actualOffset.y = Mathf.Lerp 
				(
					iOffset,
					offset.y + (floor * FloorHeigth),
					step
				);
				yield return null;
				step += Time.deltaTime / duration;
			}
		}

		private void Update () 
		{
			if (!target)
			{
				Destroy (this);
				return;
			}
			// Project [tower->hero] over XZ plane
			var forward = target.transform.position;
			forward.y = 0f;
			forward.Normalize ();

			// Compute the rotation matrix to lately extract local offset based
			// on a space that always looks outside of the circle 
			var mat = Matrix4x4.Rotate (Quaternion.LookRotation (forward));

			// Lerp side-floor-offset more smoothly
			actualOffset = new Vector3 (actualOffset.x, actualOffset.y, offset.z);	
			// Depends on moving direction
			actualOffset.x = Mathf.Lerp 
			(
				actualOffset.x,
				(target.movingDir > 0f) ? +offset.x : -offset.x,
				Time.deltaTime * 2f
			);
			
			// Get the final position (+floor height)
			var pos = target.transform.position;
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
