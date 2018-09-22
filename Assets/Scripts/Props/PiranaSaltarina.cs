using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class /* SERVER-ONLY */ PiranaSaltarina : NetBehaviour 
	{
		#region DATA
		public const float Delay = 1f;
		private const float Force = 3.5f; 
		private Rigidbody body;

		private Vector3 position;
		private Quaternion rotation;
		#endregion

		private IEnumerator Throw () 
		{
			// Wait Delay time
			float mark = Time.time + Delay;
			while (Time.time <= mark) yield return null;

			body.AddForce (Vector3.up * Force, ForceMode.VelocityChange);
			body.isKinematic = false;
		}

		[ServerCallback]
		private void Update () 
		{
			// If below starting point, freeze until new throw
			if (body.position.y <= position.y) 
			{
				transform.position = position;
				transform.rotation = rotation;
				body.isKinematic = true;
				StartCoroutine (Throw ());
			}
		}

		protected override void OnServerAwake () 
		{
			// Cache transform to control the movement
			position = transform.position;
			rotation = transform.rotation;

			body = GetComponent<Rigidbody> ();
			StartCoroutine (Throw ());
		}
	} 
}
