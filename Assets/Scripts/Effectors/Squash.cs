using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace.Effectors 
{
	// Squash works different from
	// other CCs, plus it can't be cleaned
	public class Squash : MonoBehaviour  
	{
		private IEnumerator Squashing (Hero target) 
		{
			target.anim.SetTrigger ("Squash");
			target.mods.Block ("Squashed", CCs.All);
			yield return new WaitForSeconds (1.2f);
			target.mods.Unblock ("Squashed");
		}

		private IEnumerator Pause () 
		{
			var anim = GetComponentInParent<Animator> ();
			anim.SetFloat ("SpeedMul", 0f);
			yield return new WaitForSeconds (0.75f);
			anim.SetFloat ("SpeedMul", 1f);
		}

		protected void OnTriggerEnter (Collider other) 
		{
			if (other.tag != "Player") return;
			var hero = other.GetComponent<Driver> ().owner;

			// If hit an immune Hero, temporally stops the Apisonadora
			if (hero.Immune) StartCoroutine (Pause ());
			else StartCoroutine (Squashing (hero));
		}

		private void Awake () 
		{
			// Effectors are only present on Server
			if (NetworkClient.active)
				Destroy (this);
		}
	} 
}
