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
			this.chosen = chosen;
			var plats = transform.GetChild (1);
			if (chosen)
			{
				// Enable okay platform & up-floor
				plats.GetChild (1).gameObject.SetActive (true);
				transform.GetChild (0).gameObject.SetActive (true);
			}
			else
			{
				// Enable broken platform & register all pieces
				plats.GetChild (0).gameObject.SetActive (true);
				pieces = plats.GetChild (0).GetComponentsInChildren<Rigidbody> ();
			}
		}

		[ServerCallback]
		private void Break () 
		{
			if (isServer)
			{
				// Only breakable lifts
				if (chosen) return;
				// Clients always break since it's Server-driven
				else Rpc_Break ();
			}

			foreach (var p in pieces) 
			{
				p.isKinematic = false;
				float force = Random.Range (3f, 5f);
				float upForce = Random.Range (0.5f, 1.5f);
				p.AddExplosionForce (force, transform.position, 1.5f, upForce, ForceMode.VelocityChange);
			}
		}
		[ClientRpc]
		private void Rpc_Break () { Break (); }

		private void OnTriggerEnter (Collider other) 
		{
			if (other.tag != "player") return;
			playersIn.Add (other.transform);
			PlayersIn = (playersIn.Count > 0);
		}
		private void OnTriggerExit (Collider other) 
		{
			if (other.tag != "player") return;
			playersIn.Remove (other.transform);
			PlayersIn = (playersIn.Count > 0);
		}

		protected override void OnServerAwake () 
		{
			anim = GetComponent<Animator> ().GoSmart (networked: true);
			playersIn = new List<Transform> (3);
		}
	} 
}
