using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class Q11 : QBase 
	{
		public bool spawnLifts;
		public Ascensor liftPrefab;
		public Transform[] liftSpawns;

		private void SpawnLifts () 
		{
			// Select which Ascensor is the good one
			int chosen = Random.Range (0, 3);
			for (int i=0; i!=3; ++i)
			{
				var lift = Instantiate (liftPrefab);
				lift.transform.position = liftSpawns[i].position;
				lift.transform.rotation = liftSpawns[i].rotation;
				lift.SetUp (i == chosen);
				NetworkServer.Spawn (lift.gameObject);
			}
		}

		protected override void OnAwake () 
		{
			if (NetworkClient.active)
			{
				// Register Ascensor & self for later spawning
				ClientScene.RegisterPrefab (liftPrefab.gameObject);
				ClientScene.RegisterPrefab (gameObject);
			}
			else
			// Manual override for generating lifts
			// if not using the Tower generator
			if (NetworkServer.active && spawnLifts)
				SpawnLifts ();
		}
	} 
}
