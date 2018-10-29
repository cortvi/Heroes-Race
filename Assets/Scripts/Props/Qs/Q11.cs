using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class Q11 : QBase 
	{
		public Ascensor liftPrefab;
		public Transform[] liftSpawns;

		protected override void OnServerAwake () 
		{
			// Select which Ascensor is the good one
			int chosen = Random.Range (0, 3);
			for (int i=0; i!=3; ++i)
			{
				var lift = Instantiate (liftPrefab);
				lift.transform.position = liftSpawns[i].position;
				lift.transform.rotation = liftSpawns[i].rotation;
				NetworkServer.Spawn (lift.gameObject);
				lift.SetUp (i == chosen);
			}
		}

		protected override void OnClientAwake () 
		{
			// Register Ascensor & self for later spawning
			ClientScene.RegisterPrefab (liftPrefab.gameObject);
			ClientScene.RegisterPrefab (gameObject);
		}
	} 
}
