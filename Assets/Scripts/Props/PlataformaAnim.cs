using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroesRace.Effectors 
{
	public class PlataformaAnim : NetAnchor 
	{
		public BoxCollider trigger;

		private void LateUpdate () 
		{
			// Keep trigger box always with the platform
			trigger.center = anchor.localPosition + Vector3.up * 0.128f;
		}

		private void OnTriggerEnter (Collider other) 
		{
			if (other.tag != "Player") return;
			var hero = other.GetComponent<Driver> ().owner;
			Attach (hero, useDriver: true);
		}

		private void OnTriggerExit (Collider other) 
		{
			if (other.tag != "Player") return;
			var hero = other.GetComponent<Driver> ().owner;
			Dettach (hero, useDriver: true);
		}
	} 
}
