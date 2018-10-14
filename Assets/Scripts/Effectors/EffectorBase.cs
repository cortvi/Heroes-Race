﻿using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace.Effectors
{
	public abstract class EffectorBase : MonoBehaviour 
	{
		protected abstract bool WhenImmune ();

		private void OnTriggerEnter (Collider other) 
		{
			if (other.tag != "Player") return;
			var hero = other.GetComponent<Driver> ().owner;
			if (hero.cc.immune && !WhenImmune ()) return;
			OnEnter (hero);
		}
		protected virtual void OnEnter (Hero hero) { }

		private void OnTriggerExit (Collider other) 
		{
			if (other.tag != "Player") return;
			var hero = other.GetComponent<Driver> ().owner;
			OnExit (hero);
		}
		protected virtual void OnExit (Hero hero) { }

		private void Awake () 
		{
			// Effectors are only present on Server
			if (NetworkClient.active)
				Destroy (this);
		}
	} 
}
