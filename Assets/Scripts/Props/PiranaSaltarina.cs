using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class /* SERVER-ONLY */ PiranaSaltarina : NetBehaviour 
	{
		#region DATA
		private const float JumpForce = 9f;
		private const float Delay = 1f;
		private Rigidbody body;

		private bool done;
		private Vector3 position;
		private Quaternion rotation;
		#endregion

		private IEnumerator Throw () 
		{
			// Wait Delay time
			float mark = Time.time + Random.Range (Delay - 0.5f, Delay + 0.3f);
			while (Time.time <= mark) yield return null;

			// Throw in the air
			body.useGravity = true;
			body.AddForce (Vector3.up * JumpForce, ForceMode.Impulse);
			body.AddTorque (transform.right * -2f, ForceMode.Impulse);

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
				Sleep ();

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
			Sleep ();
		}

		private void Sleep () 
		{
			body.velocity = Vector3.zero;
			body.angularVelocity = Vector3.zero;
			body.useGravity = false;
			body.Sleep ();
		}
	} 
}
