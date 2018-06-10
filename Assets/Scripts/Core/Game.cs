using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Game : NetBehaviour
{
	#region DATA 
	public enum Heroes 
	{
		NONE = -1,
		Espectador,
		Indiana,
		Harley,
		Harry,

		Count
	}
	internal Heroes playingAs;
	#endregion

	#region HELPERS
	// Spawns a hero & authorizes calling Client
	public void SpawnHero (Heroes heroToSpawn) 
	{
		// Instantiate Hero object
		var prefab = Resources.Load<Character> ("Prefabs/Heroes/" + heroToSpawn.ToString ());
		var hero = Instantiate (prefab);

		// Set up
		hero.identity = heroToSpawn;
		hero.netName = heroToSpawn.ToString ();

		// Network spawn
		NetworkServer.SpawnWithClientAuthority (hero.gameObject, connectionToClient);
	}
	#endregion
}
