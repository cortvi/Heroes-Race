﻿using System.Collections;
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
				p.AddExplosionForce (force, anchor.position, 1.5f, upForce, ForceMode.VelocityChange);
			}

			if (Net.isServer) 
			{
				// Disable platform colliders
				GetComponent<Collider> ().enabled = false;
				anchor.GetComponent<Collider> ().enabled = false;

				// Trigger exit on all Heroes
				foreach (var h in heroesIn)
				{
					h.mods.Unblock (BlockName);
					Dettach (h, useDriver: true);
				}
				heroesIn.Clear ();
				PlayersIn = false;
			}
		}

		#region CALLBACKS
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
			hero.mods.Block (BlockName, CCs.Jumping);
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
			hero.mods.Unblock (BlockName);
		}

		protected override void OnAwake () 
		{
			anim = GetComponent<Animator> ().GoSmart (networked: true);
			if (Net.isServer) heroesIn = new List<Hero> (3);
		}
		#endregion
	} 
}
