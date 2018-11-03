using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class LevelSwitch : MonoBehaviour 
	{
		private void OnTriggerEnter (Collider other) 
		{
			if (other.tag != "Player") return;
			var hero = other.GetComponent<Driver> ().owner;

			// Switch the camera level on both Client & Server
			int delta = (name.Contains ("Okay")? +1 : -1);
			var conn = hero.owner.connectionToClient;
			hero.Target_SwitchCamLevel (conn, delta);

			StartCoroutine (TowerCamera.i.tracking.SwitchFloor (delta));
		}

		private void Awake () 
		{
			// Useless on Clients
			if (Net.isClient) Destroy (this);
		}
	} 
}
