using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

// This is the "Local players" class
public class Game : NetBehaviour
{
	#region DATA
	[SyncVar] internal Heroes playingAs;
	#endregion

	#region HELPERS
	[Server] public void SpawnHero () 
	{
		// Instantiate Hero object
		var prefab = Resources.Load<Character> ("Prefabs/Heroes/" + playingAs.ToString ());
		var hero = Instantiate (prefab);

		// Set up
		hero.identity = playingAs;
		hero.SetName (playingAs.ToString ());

		// Network spawn
		NetworkServer.SpawnWithClientAuthority (hero.gameObject, connectionToClient);
	}

	public enum Heroes 
	{
		NONE = -1,

		Espectador,
		Indiana,
		Harley,
		Harry,

		Count
	}
	#endregion
}
