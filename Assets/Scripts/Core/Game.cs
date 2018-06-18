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
		hero.identity = playingAs;

		// Network spawn
		NetworkServer.SpawnWithClientAuthority (hero.gameObject, connectionToClient);
		hero.SetName (playingAs.ToString ());
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
