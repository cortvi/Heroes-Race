using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroesRace 
{
	public class Esponja : NetBehaviour 
	{
		public Transform muslce;
		public const float ThrowForce = 8f;
		private List<Rigidbody> heroesIn;

		// ——— Animator call ———
		private void ThrowHeroes () 
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

			// Attach to muscle
			other.transform.SetParent (muslce, true);
		}
		private void OnTriggerExit (Collider other)
		{
			// Unregister Hero
			if (other.tag != "Player") return;
			heroesIn.Remove (other.GetComponent<Rigidbody> ());

			// Dettach
			other.transform.SetParent (null, true);
		}

		protected override void OnAwake ()
		{
			heroesIn = new List<Rigidbody> (Net.PlayersNeeded);
		}
	}  
	#endregion
}
