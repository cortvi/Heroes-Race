using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// This is the "Local players" class
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
	[Server] public void SpawnHero (Heroes heroToSpawn) 
	{
		// Instantiate Hero object
		var prefab = Resources.Load<Character> ("Prefabs/Heroes/" + heroToSpawn.ToString ());
		var hero = Instantiate (prefab);

		// Set up
		hero.identity = heroToSpawn;
		hero.SetName (heroToSpawn.ToString ());

		// Network spawn
		NetworkServer.SpawnWithClientAuthority (hero.gameObject, connectionToClient);
	}
	#endregion
}
