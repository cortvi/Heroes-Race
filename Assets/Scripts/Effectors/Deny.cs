using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace.Effectors 
{
	public class Deny : MonoBehaviour 
	{
		[FlagEnum]
		public CCs impairing;
		private string blockName;

		private void OnTriggerEnter (Collider other) 
		{
			if (other.tag != "Player") return;
			var hero = other.GetComponent<Driver> ().owner;
			hero.mods.Add (blockName, impairing);
		}
		private void OnTriggerExit (Collider other) 
		{
			if (other.tag != "Player") return;
			var hero = other.GetComponent<Driver> ().owner;
			hero.mods.Remove (blockName);
		}

		private void Awake () 
		{
			if (Net.IsClient) Destroy (this);
			else blockName = "Deny" + impairing + Time.frameCount;
		}
	} 
}
