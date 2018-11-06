using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class Ascensor : NetBehaviour 
	{
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
		private List<Transform> playersIn;

		private void Break () 
		{
			// Only broken lifts
			if (Chosen) return;

			// Explode into pieces
			var plats = transform.GetChild (2);
			var pieces = GetComponentsInChildren<Rigidbody> ();
			foreach (var p in pieces) 
			{
				p.isKinematic = false;
				float force = Random.Range (4f, 7f);
				float upForce = Random.Range (0.2f, 0.7f);
				p.AddExplosionForce (force, plats.position, 1.5f, upForce, ForceMode.VelocityChange);
			}

			// Disable platform colliders
			if (Net.isServer)
			{
				GetComponent<Collider> ().enabled = false;
				plats.GetComponent<Collider> ().enabled = false;
			}
		}

		private void Attach (bool attach, Transform hero) 
		{
			if (Net.isServer) Rpc_Attach (attach);
			hero.SetParent (attach? transform : null, true);
		}
		[ClientRpc]
		private void Rpc_Attach (bool attach) 
		{
			// Attaches / Dettaches Hero to lift
			Attach (attach, Net.me.pawn.transform);
		}

		#region CALLBACKS
		[ServerCallback]
		private void OnTriggerEnter (Collider other) 
		{
			if (other.tag != "Player") return;
			playersIn.Add (other.transform);
			PlayersIn = (playersIn.Count > 0);
			Attach (true, other.transform);
		}
		[ServerCallback]
		private void OnTriggerExit (Collider other) 
		{
			if (other.tag != "Player") return;
			playersIn.Remove (other.transform);
			PlayersIn = (playersIn.Count > 0);
			Attach (false, other.transform);
		}

		protected override void OnAwake () 
		{
			anim = GetComponent<Animator> ().GoSmart (networked: true);
			if (Net.isServer) playersIn = new List<Transform> (3);
		} 
		#endregion
	} 
}
