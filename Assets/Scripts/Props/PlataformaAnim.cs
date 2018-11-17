using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace.Effectors 
{
	public class PlataformaAnim : NetAnchor 
	{
		public BoxCollider trigger;

		[ServerCallback]
		private void LateUpdate () 
		{
			// Keep trigger box always with the platform
			trigger.center = anchor.localPosition + Vector3.up * 0.128f;
		}

		[ServerCallback]
		private void OnTriggerEnter (Collider other) 
		{
			if (other.tag != "Player") return;
			var hero = other.GetComponent<Driver> ().owner;
			Attach (hero, useDriver: true);
		}

		[ServerCallback]
		private void OnTriggerExit (Collider other) 
		{
			if (other.tag != "Player") return;
			var hero = other.GetComponent<Driver> ().owner;
			Dettach (hero, useDriver: true);
		}
	} 
}
