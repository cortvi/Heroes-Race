using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroesRace.Effectors 
{
	public class TentaculoGrab : MonoBehaviour 
	{
		private NetAnchor anchor;
		internal Hero grabbed;

		private void OnTriggerEnter (Collider other) 
		{
			// If touched Player and hasn't grabbed anyone yet
			if (grabbed || other.tag != "Player") return;
			var hero = other.GetComponent<Driver> ().owner;
			if (!hero.Immune) 
			{
				// Grab Hero
				anchor.Attach (hero, useDriver: false);
				hero.transform.position = transform.position;
				hero.transform.rotation = transform.rotation;
				grabbed = hero;

				// Disable Driver until released
				hero.driver.enabled = false;
			}
			else hero.mods.Add ("Shield Break", CCs.None, 0.1f);
		}

		private void Awake () 
		{
			if (Net.IsServer) anchor = GetComponentInParent<NetAnchor> ();
			else
			// Useless on Clients
			if (Net.IsClient) Destroy (this);
		}
	} 
}
