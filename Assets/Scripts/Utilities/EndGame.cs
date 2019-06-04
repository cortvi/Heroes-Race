using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroesRace 
{
	public class EndGame: MonoBehaviour 
	{
		private bool isOver;

		private void OnTriggerEnter (Collider other) 
		{
			if (isOver || other.tag != "Player") return;
			var player = other.GetComponent<Hero> ().owner;

			Net.EndItAll (player);
			isOver = true;
		}

		private void Awake ()
		{
			// Useless on clients:
			if (Net.IsClient) Destroy (this);
	}
	} 
}
