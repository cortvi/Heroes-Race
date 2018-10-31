using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class Ascensor : NetBehaviour 
	{
		private SmartAnimator anim;
		private List<Transform> playersIn;
		private bool PlayersIn 
		{
			set { anim.SetBool ("PlayersIn", value); }
		}

		private bool chosen;
		private Rigidbody[] pieces;

		public void SetUp (bool chosen) 
		{
			//  Set up clients too
			if (Net.isServer) Rpc_SetUp (chosen);
			this.chosen = chosen;

			var plats = transform.GetChild (3);
			if (chosen)
			{
				// Enable okay platform, up-floor & level switch
				plats.GetChild (1).gameObject.SetActive (true);
				transform.GetChild (0).gameObject.SetActive (true);
				transform.GetChild (2).gameObject.SetActive (true);
			}
			else
			{
				// Enable broken platform, level switch & register all pieces
				plats.GetChild (0).gameObject.SetActive (true);
				transform.GetChild (1).gameObject.SetActive (true);
				pieces = plats.GetChild (0).GetComponentsInChildren<Rigidbody> ();
			}
		}
		[ClientRpc]
		private void Rpc_SetUp (bool chosen) { SetUp (chosen); }

		[ServerCallback]
		private void Break () 
		{
			if (Net.isServer) 
			{
				// Only breakable lifts
				if (chosen) return;
				// Clients always break since it's Server-driven
				else Rpc_Break ();
			}

			// Explode
			var plats = transform.GetChild (3);
			foreach (var p in pieces) 
			{
				p.isKinematic = false;
				float force = Random.Range (4f, 7f);
				float upForce = Random.Range (0.2f, 0.7f);
				p.AddExplosionForce (force, plats.position, 1.5f, upForce, ForceMode.VelocityChange);
			}

			// Disable all platform colliders
			foreach (var c in plats.GetComponentsInChildren<Collider> ())
				c.enabled = false;
		}
		[ClientRpc]
		private void Rpc_Break () { Break (); }

		#region CALLBACKS
		[ServerCallback]
		private void OnTriggerEnter (Collider other) 
		{
			if (other.tag != "Player") return;
			playersIn.Add (other.transform);
			PlayersIn = (playersIn.Count > 0);
		}
		[ServerCallback]
		private void OnTriggerExit (Collider other) 
		{
			if (other.tag != "Player") return;
			playersIn.Remove (other.transform);
			PlayersIn = (playersIn.Count > 0);
		}

		protected override void OnServerAwake () 
		{
			anim = GetComponent<Animator> ().GoSmart (networked: true);
			playersIn = new List<Transform> (3);
		} 
		#endregion
	} 
}
