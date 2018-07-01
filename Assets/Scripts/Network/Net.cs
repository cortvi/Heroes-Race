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
		public static List<Player> players;
		public static Player me;

		public static bool isServer;
		public static bool isClient;

		public const int UsersNeeded = 1;

		private int clientsReady;
		#endregion

		#region SERVER
		public override void OnServerAddPlayer (NetworkConnection conn, short playerControllerId) 
		{
			// Check if it's the first time the player connects
			var player = players.FirstOrDefault (p=> p.connectionToClient == conn);
			if (player == null) 
			{
				// Create a new persistent Player object
				print ("Creating new persistent Player");
				player = Instantiate (playerPrefab).GetComponent<Player> ();

				// Spawn it over the net
				NetworkServer.AddPlayerForConnection (conn, player.gameObject, playerControllerId);
				players.Add (player);
			}
			else
			{
				print ("Re-connecting Player...");
				// how do I re-authorize...?
			}
		}

		public override void OnServerReady (NetworkConnection conn) 
		{
			NetworkServer.SetClientReady (conn);
			if (players.Count != 0) 
			{
				// Notify Players that the scene is ready on both sides
				var player = players.First(p=> p.connectionToClient == conn);
				player.SceneReady ();
			}
			print ("Player " + conn.connectionId + " is ready now");
		}

		public override void OnServerDisconnect (NetworkConnection conn) 
		{
			// Don't remove owned objects
			print ("Player " + conn.connectionId + " disconnected from server!");
		}

		public override void OnServerSceneChanged (string sceneName) 
		{
			base.ServerChangeScene (sceneName);
			// do something else? (=> this is actually kind of a loop)
		}

		public override void OnStartServer () 
		{
			base.OnStartServer ();
			isServer = true;
		}
		public override void OnStopServer () 
		{
			base.OnStopServer ();
			isServer = false;
			players.Clear ();
		}
		#endregion

		#region CLIENT
		public override void OnClientSceneChanged (NetworkConnection conn) 
		{
			// Set players ready
			ClientScene.Ready (conn);
		}

		public override void OnStartClient (NetworkClient client) 
		{
			base.OnStartClient (client);
			isClient = true;
		}
		public override void OnStopClient () 
		{
			base.OnStopClient ();
			isClient = false;
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
		[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void InitizalizeSingleton () 
		{
			// Creates a persistent Net-worker no matter the scene
			worker = Extensions.SpawnSingleton<Net> ("Networker");
			players = new List<Player> (3);
		}

		private void SceneLogic () 
		{
			/*
			// Behaviour based on what scene we start at
			if (scene == "Selection")
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
