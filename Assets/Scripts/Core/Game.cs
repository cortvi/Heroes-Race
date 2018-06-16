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
	internal Heroes playingAs;
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
		// Put player in start position
		hero.driver.rotation = Quaternion.Euler (0f, 203.91f, 0f);

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
