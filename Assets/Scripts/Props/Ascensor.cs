using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class Ascensor : NetAnchor 
	{
		#region DATA
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
		private List<Hero> heroesIn;
		private const string BlockName = "Don't jump on lifts!";

		private BoxCollider trigger;
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
				float force = Random.Range (2f, 5f);
				float upForce = Random.Range (0.2f, 1f);
				p.AddExplosionForce (force, anchor.position, 1.5f, upForce, ForceMode.VelocityChange);
				Destroy (p, 2f);
			}

			if (Net.isServer) 
			{
				// Disable all lift colliders
				anchor.GetComponent<Collider> ().enabled = false;
				trigger.enabled = false;

				// Trigger exit on all Heroes & throw on air
				foreach (var h in heroesIn)
				{
					h.mods.Remove (BlockName);
					Dettach (h, useDriver: true);

					// Throw player on air
					h.driver.body.AddForce (Vector3.up * 3f, ForceMode.VelocityChange);
					h.driver.SwitchFriction (false);
					h.OnAir = true;
				}
				heroesIn.Clear ();
				PlayersIn = false;
			}
		}

		#region CALLBACKS
		[ServerCallback]
		private void LateUpdate () 
		{
			// Keep always trigger box with platform
			trigger.center = anchor.localPosition + Vector3.forward * 0.08f;
		}

		[ServerCallback]
		private void OnTriggerEnter (Collider other) 
		{
			if (other.tag != "Player") return;
			var hero = other.GetComponent<Driver> ().owner;

			// Add to list
			heroesIn.Add (hero);
			PlayersIn = (heroesIn.Count > 0);

			// Attach Hero Driver to Lift & avoid jumping
			Attach (hero, useDriver: true); 
			hero.mods.Add (BlockName, CCs.Jumping);
		}
		[ServerCallback]
		private void OnTriggerExit (Collider other) 
		{
			if (other.tag != "Player") return;
			var hero = other.GetComponent<Driver> ().owner;

			// Remove form list
			heroesIn.Remove (hero);
			PlayersIn = (heroesIn.Count > 0);

			// Dettach Hero Driver from Lift & re-allow jump
			Dettach (hero, useDriver: true);
			hero.mods.Remove (BlockName);
		}

		protected override void OnAwake () 
		{
			anim = GetComponent<Animator> ().GoSmart (networked: true);
			if (Net.isServer)
			{
				heroesIn = new List<Hero> (3);
				trigger = GetComponent<BoxCollider> ();
			}
		}
		#endregion
	} 
}
