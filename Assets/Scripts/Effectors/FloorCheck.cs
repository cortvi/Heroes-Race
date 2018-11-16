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

			// Get the moving direction
			float height = transform.position.y;
			int dir = (int)Mathf.Sign (height - hero.lastPos.y);

			// Use a treshold for both going up & down
			float floorStart = (hero.floor + 1) * HeroCamera.FloorHeigth - 0.45f;
			float floorEnd = hero.floor * HeroCamera.FloorHeigth + 0.75f;

			// Changing 'floor' will make Camera auto-update
			if (canGoUp && dir == 1f && height > floorStart) hero.floor++;
			else
			if (canGoDown && dir == -1f && height < floorEnd) hero.floor--;
		}

		private void Awake () 
		{
			// Only work on Clients
			if (Net.IsClient) Destroy (this);
		}
	} 
}
