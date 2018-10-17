using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace.Effectors
{
	public abstract class EffectorBase : MonoBehaviour 
	{
		private bool skippedEnter;   // True when entering was skipped due to Immunity

		protected virtual void OnEnter (Hero hero) { }
		protected virtual void OnExit (Hero hero) { }

		#region CALLBACKS
		protected void OnTriggerEnter (Collider other) 
		{
			if (other.tag != "Player") return;
			var hero = other.GetComponent<Driver> ().owner;
			if (hero.Immune) skippedEnter = true;
			else OnEnter (hero);
		}

		private void OnTriggerExit (Collider other) 
		{
			if (other.tag != "Player") return;
			var hero = other.GetComponent<Driver> ().owner;
			if (skippedEnter) skippedEnter = false;
			else OnExit (hero);
		}

		private void Awake () 
		{
			// Effectors are only present on Server
			if (NetworkClient.active)
				Destroy (this);
		} 
		#endregion
	} 
}
