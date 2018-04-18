using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Game : NetworkBehaviour
{
	public static Game player;
	[SyncVar] public Heroes playingAs;

	#region CALLBACKS
	[ClientCallback] private void Update () 
	{
		print ("test");
		if (playingAs == 0 && Input.GetKey(KeyCode.I) && Input.GetKeyDown(KeyCode.LeftControl))
		{
			SpawnHero (Heroes.Indiana);
		}
	}

	/// This will set the player object reference for each Client
	public void Start () 
	{
		print (name + ":" + isLocalPlayer);
		if (isLocalPlayer)
			player = this;
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
		hero.name = "["+connectionToClient.connectionId+"] " + heroToSpawn;
		hero.identity = heroToSpawn;
		playingAs = heroToSpawn;

		/// Network spawn
		NetworkServer.SpawnWithClientAuthority (hero.gameObject, connectionToClient);
	}
	public void SpawnHero (Heroes heroToSpawn) 
	{
		Cmd_SpawnHero (heroToSpawn);
	}
	#endregion
}
