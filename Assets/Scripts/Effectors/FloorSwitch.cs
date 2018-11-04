using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{

	public class FloorSwitch : MonoBehaviour 
	{
		[Info] public int toFloor;

		private void OnTriggerEnter (Collider other) 
		{
			if (other.tag != "Player") return;
			var hero = other.GetComponent<Driver> ().owner;

			// Do nothing if Hero's already on that floor
			if (hero.floor != toFloor) hero.SwitchCamFloor (toFloor);
		}

		private void Awake () 
		{
			// Useless on Clients
			if (Net.isClient) Destroy (this);
		}
	} 
}
