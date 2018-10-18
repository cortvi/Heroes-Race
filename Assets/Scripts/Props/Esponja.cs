using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class Esponja : NetBehaviour 
	{
		#region DATA
		public Transform muslce;

		private const float ThrowForce = 8f;
		private List<Rigidbody> heroesIn;
		private Animator anim; 
		#endregion

		[ServerCallback]
		// ——— Animator call ———
		public void ThrowHeroes () 
		{
			// Apply jump force to all Heroes up in the Esponja
			var force = Vector3.up * ThrowForce;
			foreach (var h in heroesIn) h.AddForce (force, ForceMode.VelocityChange);
		}

		#region CALLBACKS
		private void OnTriggerEnter (Collider other)
		{
			// Register Hero
			if (other.tag != "Player") return;
			heroesIn.Add (other.GetComponent<Rigidbody> ());
			anim.SetBool ("Player_in", true);

			// Attach to muscle
			other.transform.SetParent (muslce, true);
		}
		private void OnTriggerExit (Collider other)
		{
			// Unregister Hero
			if (other.tag != "Player") return;
			heroesIn.Remove (other.GetComponent<Rigidbody> ());
			anim.SetBool ("Player_in", (heroesIn.Count > 0));

			// Dettach
			other.transform.SetParent (null, true);
		}

		protected override void OnServerAwake () 
		{
			anim = GetComponent<Animator> ();
			heroesIn = new List<Rigidbody> (Net.PlayersNeeded);
		}
	}  
	#endregion
}
