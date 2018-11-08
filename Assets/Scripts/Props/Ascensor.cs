using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class Ascensor : NetBehaviour 
	{
		#region DATA
		public Transform platforms;
		private SmartAnimator anim;
		internal bool Chosen 
		{
			get { return anim.GetBool ("Chosen"); }
			set { anim.SetBool ("Chosen", value); }
		}
		internal bool PlayersIn 
		{
			set { anim.SetBool ("PlayersIn", value); }
		}
		private List<Transform> heroesIn; 
		#endregion

		private void Break () 
		{
			// Only broken lifts
			if (Chosen) return;

			// Explode into pieces
			var pieces = GetComponentsInChildren<Rigidbody> ();
			foreach (var p in pieces) 
			{
				p.isKinematic = false;
				float force = Random.Range (4f, 7f);
				float upForce = Random.Range (0.2f, 0.7f);
				p.AddExplosionForce (force, platforms.position, 1.5f, upForce, ForceMode.VelocityChange);
			}

			// Disable platform colliders
			if (Net.isServer) 
			{
				GetComponent<Collider> ().enabled = false;
				platforms.GetComponent<Collider> ().enabled = false;
			}
		}

		#region CALLBACKS
		[ServerCallback]
		private void OnTriggerEnter (Collider other) 
		{
			if (other.tag != "Player") return;
			heroesIn.Add (other.transform);
			PlayersIn = (heroesIn.Count > 0);

			// Attach Hero Driver to Lift
			var hero = other.GetComponent<Driver> ().owner;
			hero.mods.Block ("Dont jump on Lifts", CCs.Jumping);
			hero.Attach (platforms, attachDrive: true); 
		}
		[ServerCallback]
		private void OnTriggerExit (Collider other) 
		{
			if (other.tag != "Player") return;
			heroesIn.Remove (other.transform);
			PlayersIn = (heroesIn.Count > 0);

			// Dettach Hero Driver from Lift & re-allow jump
			var hero = other.GetComponent<Driver> ().owner;
			hero.mods.Unblock ("Dont jump on Lifts");
			hero.Attach (null, attachDrive: true);
		}

		[ServerCallback]
		private void OnDisable () 
		{
			
		}

		protected override void OnAwake () 
		{
			anim = GetComponent<Animator> ().GoSmart (networked: true);
			if (Net.isServer) heroesIn = new List<Transform> (3);
		}
		#endregion
	} 
}
