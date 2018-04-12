using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Game : NetworkBehaviour
{
	public static Game player;

	/// This will set the player object reference for each Client
	[ClientCallback] public override void OnStartAuthority () 
	{
		base.OnStartAuthority ();
		player = this;
	}

	#region HELPERS
	/// Spawns a hero & authorizes this Client
	[Command] private void Cmd_Spawn (Heroes heroToSpawn) 
	{
		/// Instantiate Hero object
		var prefab = Resources.Load<Character> ("Prefabs/Heroes/" + heroToSpawn.ToString ());
		var hero = Instantiate (prefab);

		/// Change parameters
		hero.name = "[" + connectionToClient.connectionId + "] " + heroToSpawn;
		hero.identity = heroToSpawn;

		/// Network spawn
		NetworkServer.SpawnWithClientAuthority (hero.gameObject, connectionToClient);
	}
	public void Spawn (Heroes heroToSpawn) 
	{
		Cmd_Spawn (heroToSpawn);
	}
	#endregion
}
