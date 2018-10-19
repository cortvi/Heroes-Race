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
			if (target.mods.Block ("Squashed", CCs.All))
				target.anim.SetTrigger ("Squash");

			// Avoid crashing into stone collider
			target.driver.body.isKinematic = true;
			yield return new WaitForSeconds (1.2f);
			target.driver.body.isKinematic = false;

			target.mods.Unblock ("Squashed");
		}

		protected void OnTriggerEnter (Collider other) 
		{
			if (other.tag != "Player") return;
			var hero = other.GetComponent<Driver> ().owner;

			// If hit an immune Hero, temporally stops the Apisonadora
			if (hero.Immune)
			{
				hero.mods.AddCC ("Shieldbreak", CCs.None, 0.5f, false);
				var anim = GetComponentInParent<Apisonadora> ().anim;
				anim.SetTrigger ("Reset");
			}
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
