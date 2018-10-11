using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class Power : NetBehaviour 
	{
		#warning Should I make them re-spawn?
		[Info] public PowerUp power;

		[ServerCallback]
		private void OnTriggerEnter (Collider other) 
		{
			if (other.tag != "Player") return;
			var hero = other.GetComponent<Driver> ().owner;
			if (hero.power == PowerUp.None) 
			{
				hero.power = power;
				NetworkServer.Destroy (gameObject);
				#warning En el futuro, hace más sutil la desaparición
			}
		}

		protected override void OnStart () 
		{
			if (NetworkServer.active)
			{
				int idx = Random.Range (1, (int) PowerUp.Count);
				power = (PowerUp) idx;
			}
			else
			if (NetworkClient.active)
				Destroy (this);
		}
	} 
}
