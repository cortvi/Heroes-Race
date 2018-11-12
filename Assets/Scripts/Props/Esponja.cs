using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class Esponja : NetBehaviour 
	{
		#region DATA
		private Animator anim; 
		private List<Driver> heroesIn;

		private const float ThrowForce = 8f;
		#endregion

		[ServerCallback]
		// ——— Animator call ———
		public void ThrowHeroes () 
		{
			// Apply jump force to all Heroes up in the Esponja
			var force = Vector3.up * ThrowForce;
			foreach (var d in heroesIn) 
			{
				// Impulse each Hero upwards
				d.body.AddForce (force, ForceMode.VelocityChange);
				d.SwitchFriction (touchingFloor: false);
				d.owner.OnAir = true;
			}
		}

		#region CALLBACKS
		[ServerCallback]
		private void OnTriggerEnter (Collider other) 
		{
			// Register Hero
			if (other.tag != "Player") return;
			heroesIn.Add (other.GetComponent<Driver> ());
			anim.SetBool ("Player_in", true);
		}
		[ServerCallback]
		private void OnTriggerExit (Collider other) 
		{
			// Unregister Hero
			if (other.tag != "Player") return;
			heroesIn.Remove (other.GetComponent<Driver> ());
			anim.SetBool ("Player_in", (heroesIn.Count > 0));
		}

		protected override void OnServerAwake () 
		{
			anim = GetComponent<Animator> ();
			heroesIn = new List<Driver> (Net.PlayersNeeded);
		}
	}  
	#endregion
}
