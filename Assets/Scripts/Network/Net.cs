using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace HeroesRace 
{
	public class Net : NetworkManager 
	{
		#region DATA
		public static Net worker;
		public static List<User> users;

		public static bool isServer;
		public static bool isClient;

		public const int UsersNeeded = 2;
		#endregion

		#region SERVER
		public override void OnServerAddPlayer (NetworkConnection conn, short playerControllerId) 
		{
			// Check if Player if it's the first time the player connects
			var assignedUser = users.FirstOrDefault (u => u.IP == conn.address);
			if (assignedUser == null) 
			{
				// Spawn player object over the net
				var player = Instantiate (playerPrefab).GetComponent<Player> ();
				NetworkServer.AddPlayerForConnection (conn, player.gameObject, playerControllerId);

				// Otherwise create a new persistent User for that player
				int id = conn.connectionId;
				users.Add (new User (player, id, conn.address));

				// When all ready, stop broadcasting & start the game
				if (!isServer && users.Count == UsersNeeded)
				{
					isServer = true;
					// do something, idk
				}
			}
			else 
			{
				// In case they're just reconnecting,
				// just re-assign local-player authority
				var player = assignedUser.Player;
				NetworkServer.AddPlayerForConnection (conn, player.gameObject, playerControllerId);
			}
		}

		public override void OnServerConnect (NetworkConnection conn) 
		{
			base.OnServerConnect (conn);
		}

		public override void OnServerDisconnect (NetworkConnection conn) 
		{
			base.OnServerDisconnect (conn);
		}
		#endregion

		#region CLIENT
		public override void OnClientConnect (NetworkConnection conn) 
		{
			print ("lul");
			base.OnClientConnect (conn);
		}

		public override void OnClientDisconnect (NetworkConnection conn) 
		{
			base.OnClientDisconnect (conn);
		}
		#endregion

		#region CALLBACKS
		[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void InitizalizeSingleton () 
		{
			// Creates a persistent Net-worker no matter the scene
			worker = Extensions.SpawnSingleton<Net> ("Networker");
		}
		#endregion

		#region TOWER LOADING
		public void GoToTower () 
		{
		/*
			// Read all the selected heroes
			Player.Heroes[] heroes = new Player.Heroes[3];
			for (int i = 0; i != players.Count; i++)
			{
				var selector = GameObject.Find ("[" + (i + 1) + "][CLIENT] Selector");
				heroes[i] = selector.GetComponent<Selector> ().ReadHero ();
			}
			// Save hero selection
			for (int i = 0; i != players.Count; i++)
				players[i].playingAs = heroes[i];
			
			// Change scene
			StartCoroutine (LoadTower ());
		}
		private IEnumerator LoadTower () 
		{
			ServerChangeScene ("Tower");
			// Wait until ALL players are ready
			yield return new WaitUntil (()=> players.All (p => p.id.connectionToClient.isReady));

			// Spawn all heroes with authorization
			foreach (var p in players) p.SpawnHero ();
		*/
		}
		#endregion

		#region HELPERS
		private void SceneLogic () 
		{
			/*
			// Behaviour based on what scene we start at
			if (scene == "Menu")
			{
				// Assign authority to selector
				var selector = GameObject.Find ("Selector_" + conn.connectionId).GetComponent<Selector> ();
				selector.id.AssignClientAuthority (conn);
				selector.UpdateName ();
			}
			else
			// If bypassing the Selection menu
			if (scene == "Tower")
			{
				// Spawn a different hero for each player & start

			}
			*/
		}
		#endregion
	} 
}
