using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroesRace.Effectors 
{
	public class FloorCheck : MonoBehaviour 
	{
		public bool canGoUp;
		public bool canGoDown;

		private void OnTriggerStay (Collider other) 
		{
			if (other.tag != "Player") return;
			var hero = other.GetComponent<Driver> ().owner;
			float height = hero.transform.position.y;

			// Use a treshold for both going up & down
			float floorStart = hero.floor * HeroCam.FloorHeigth + 0.75f;
			float floorEnd = (hero.floor + 1) * HeroCam.FloorHeigth - 0.45f;

			// Changing 'floor' will make Camera auto-update
			if (canGoUp && hero.vDir.Is (1f) && height > floorEnd) hero.floor++;
			else
			if (canGoDown && hero.vDir.Is (-1f) && height < floorStart) hero.floor--;
		}

		private void Awake () 
		{
			// Only work on Clients
			if (Net.IsClient) Destroy (this);
		}
	} 
}
