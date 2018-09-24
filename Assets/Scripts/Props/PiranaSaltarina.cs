using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class /* SERVER-ONLY */ PiranaSaltarina : NetBehaviour 
	{
		#region DATA
		public float KnockForce = 8.75f;
		private const float JumpForce = 8.75f;
		private const float Delay = 1f;
		private Rigidbody body;

		private bool done;
		private Vector3 position;
		private Quaternion rotation;
		private Matrix4x4 knockDirHelper;
		#endregion

		private IEnumerator Throw () 
		{
			// Wait Delay time
			float mark = Time.time + Delay;
			while (Time.time <= mark) yield return null;

			// Throw in the air
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

		[ServerCallback]
		private void OnTriggerEnter (Collider other) 
		{
			var hero = other.GetComponent<Hero> ();
			if (hero != null)
			{
				// Compute knock direction
				var mat = Matrix4x4.TRS (transform.position, rotation, Vector3.one);
//				mat.
			}
		}

		protected override void OnServerAwake () 
		{
			// Cache transform to control the movement
			position = transform.position;
			rotation = transform.rotation;

			// Create TRS matrix to check 
			var q = Quaternion.Euler (90, rotation.eulerAngles.y, 180f);   // Use same rotation to 
			knockDirHelper = Matrix4x4.Inverse (Matrix4x4.TRS (position, , Vector3.one);

			body = GetComponent<Rigidbody> ();
			StartCoroutine (Throw ());
		}

		private void Sleep () 
		{
			body.velocity = Vector3.zero;
			body.angularVelocity = Vector3.zero;
			body.Sleep ();
		}
	} 
}
