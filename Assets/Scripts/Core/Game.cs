using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Game : NetBehaviour
{
	#region CALLBACKS
	[ClientCallback]
	private void Update () 
	{
		if (Input.GetKeyDown (KeyCode.I) && Input.GetKey (KeyCode.LeftControl)) SpawnHero (Heroes.Indiana);
	}
	#endregion

	#region HELPERS
	/// Spawns a hero & authorizes this Client
	[Command] private void Cmd_SpawnHero (Heroes heroToSpawn) 
	{
		/// Instantiate Hero object
		var prefab = Resources.Load<Character> ("Prefabs/Heroes/" + heroToSpawn.ToString ());
		var hero = Instantiate (prefab);

		/// Set up
		hero.netName = "["+connectionToClient.connectionId+"] " + heroToSpawn;
		hero.identity = heroToSpawn;

		/// Network spawn
		NetworkServer.SpawnWithClientAuthority (hero.gameObject, connectionToClient);
		hero.Target_SetLocal (connectionToClient);
	}
	public void SpawnHero (Heroes heroToSpawn) 
	{
		Cmd_SpawnHero (heroToSpawn);
	}
	#endregion
}
