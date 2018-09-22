using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class /* SERVER-ONLY */ PiranaSaltarina : NetBehaviour 
	{
		#region DATA
		private const float Force = 8.5f; 
		private const float Delay = 1f;
		private Rigidbody body;

		private Vector3 position;
		private Quaternion rotation;
		private bool done;
		#endregion

		private IEnumerator Throw () 
		{
			// Wait Delay time
			float mark = Time.time + Delay;
			while (Time.time <= mark) yield return null;

			// Throw in the air
			body.isKinematic = false;
			body.AddForce (Vector3.up * Force, ForceMode.Impulse);

			// Wait a bit for it to fly
			mark = Time.time + 0.75f;
			while (Time.time <= mark) yield return null;

			// Wait until is lands on floor again
			while (transform.position.y > position.y) yield return null;
			done = true;
		}

		[ServerCallback]
		private void Update () 
		{
			if (done) 
			{
				transform.position = position;
				transform.rotation = rotation;
				body.isKinematic = true;

				StartCoroutine (Throw ());
				done = false;
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
