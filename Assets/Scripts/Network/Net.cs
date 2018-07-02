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
//		public static Player me;

		public static bool isServer;
		public static bool isClient;

		public const int UsersNeeded = 1;
		public int ClientsReady 
		{
			get { return users.Count (u => u.ready); }
		}
		#endregion

		#region SERVER
		public override void OnServerAddPlayer (NetworkConnection conn, short playerControllerId) 
		{
			// Spawn Player object over the net
			var player = Instantiate (playerPrefab).GetComponent<Player> ();
			NetworkServer.AddPlayerForConnection (conn, player.gameObject, playerControllerId);

			// Check if it's the first time the player connects
			var user = users.FirstOrDefault (u => u.IP == conn.address);
			if (user == null)
			{
				// Create a new persistent Player object
				print ("Creating new persistent User");
				user = new User (player);
				users.Add (user);
			}
			else print ("Player " + user.ID + " just reconnected!");

			// Assign Player to the User
			user.AssignPlayer (player);
		}

		public override void OnServerReady (NetworkConnection conn) 
		{
			base.OnServerReady (conn);
			print ("Does this happend before Player object spawn..?");
		}

		public override void ServerChangeScene (string newSceneName) 
		{
			// Set all users un-ready
			users.ForEach (u => u.ready = false);
			base.ServerChangeScene (newSceneName);
		}

		public override void OnServerDisconnect (NetworkConnection conn) 
		{
			// Set user un-ready
			var user = users.Find (u => u.IP == conn.address);
			print ("Player " + user.ID + " disconnected from server!");
			user.ready = false;

			// This will destroy the user Player object
			base.OnServerDisconnect (conn);
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
			users.Clear ();
		}
		#endregion

		#region CLIENT
		public override void OnClientSceneChanged (NetworkConnection conn) 
		{
			// This calls ClientScene.Ready
			// Instead, I'm calling it with a delegate to the SceneManager
		}

		public override void OnClientNotReady (NetworkConnection conn) 
		{
			// was this enforcing auto-reconnect?
			//nope.
		}

		public override void OnStartClient (NetworkClient client) 
		{
			isClient = true;
		}
		public override void OnStopClient () 
		{
			isClient = false;
		}
		#endregion

		#region CALLBACKS
		private void OnDestroy () 
		{
			print ("Networker destroyed!");
			SceneManager.sceneLoaded -= SceneReady;
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
		public void SceneReady (Scene scene, LoadSceneMode mode) 
		{
			if (isClient && !client.connection.isReady)
				ClientScene.Ready (client.connection);
		}

		[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void InitizalizeSingleton () 
		{
			// Creates a persistent Net-worker no matter the scene
			worker = Extensions.SpawnSingleton<Net> ("Networker");
			SceneManager.sceneLoaded += worker.SceneReady;
			users = new List<User> (3);
		}

		private void SceneLogic () 
		{
			// Behaviour based on what scene we start at
			if (networkSceneName == "Selection") 
			{
				print ("Assigning authority to Selectors");
				var selectors = FindObjectsOfType<Selector> ();
				foreach (var u in users) 
				{
					var selector = selectors[u.ID - 1];
					selector.id.AssignClientAuthority (u.Player.connectionToClient);
					selector.UpdateName ();
				}
			}
			else
			// If bypassing the Selection menu
			if (networkSceneName == "Tower") 
			{
				// Spawn a different hero for each player & start !
			}
		}
		#endregion
	} 
}
