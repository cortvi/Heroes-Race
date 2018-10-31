using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class /* SERVER-ONLY */ Power : NetBehaviour 
	{
		[Info] public PowerUp power;
		private SmartAnimator anim;
		private const float respawnTime = 2f;

		private IEnumerator Consume () 
		{
			// Make it dissapepear
			yield return new WaitForSeconds (1f);
			NetworkServer.UnSpawn (gameObject);

			// Make it re-appear again
			yield return new WaitForSeconds (respawnTime);
			NetworkServer.Spawn (gameObject);
			anim.SetTrigger ("Reset");
			SetPower ();

			#warning Delete this if Power name is OK
//			UpdateName ();
		}
		private void SetPower () 
		{
			int idx = Random.Range (1, (int) PowerUp.Count);
			power = (PowerUp)idx;
		}

		#region CALLBACKS
		private void OnTriggerEnter (Collider other) 
		{
			if (other.tag != "Player") return;
			var hero = other.GetComponent<Driver> ().owner;
			if (hero.power == PowerUp.None)
			{
				hero.power = power;
				anim.SetTrigger ("Consume");
				StartCoroutine (Consume ());
			}
		}

		protected override void OnStart () 
		{
			if (Net.isServer)
			{
				SetPower ();
				anim = GetComponent<Animator> ()
					  .GoSmart (networked: true);
			}
			else
			// Only works on Server
			if (Net.isClient)
				Destroy (this);
		} 
		#endregion
	} 
}
