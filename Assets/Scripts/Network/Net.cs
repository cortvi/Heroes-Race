using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using SM = UnityEngine.SceneManagement.SceneManager;

namespace HeroesRace 
{
	public partial class /* COMMON */ Net : NetworkManager 
	{
		#region DATA
		public static Net worker;
		public static int PlayersNeeded { get; private set; }
		public static bool isClient;
		public static bool isServer;
		public static bool paused;

		public Animator courtain;
		public string firstScene; 
		#endregion

		[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void Main () 
		{
			// Initialize some basic aspects
			worker = Extensions.SpawnSingleton<Net> ("Networker");
			Log.logLevel = Log.LogType.DeepDebug;
			Application.targetFrameRate = 60;

			worker.courtain = Instantiate (worker.courtain);
			worker.courtain.name = "Courtain";

			// Read config:
			string[] config = File.ReadAllLines (Application.streamingAssetsPath + "/config.txt");
			worker.StartCoroutine (worker.Config (config));
		}

		private IEnumerator Config (string[] config) 
		{
			// Ensure we start in the base scene
			if (SM.GetActiveScene ().name != "!Zero") 
			{
				yield return SM.LoadSceneAsync ("!Zero");
				yield return new WaitForSeconds (1f);
			}

			if (config[0] == "client") 
			{
				// Start Client from given address
				networkAddress = config[1].Trim ();
				StartClient ();
				Log.LowDebug ("This mahcine is now a client");
				isClient = true;
			}
			else
			if (config[0] == "server") 
			{
				// Start Server to expect given Players
				PlayersNeeded = int.Parse (config[1]);
				players = new Player[PlayersNeeded];

				StartServer ();
				Log.LowDebug ("This mahcine is now the server");
				isServer = true;

				// Spawn Courtain controller
				var cRig = Instantiate (Resources.Load ("Prefabs/Courtain_Controller"));
				NetworkServer.Spawn (cRig as GameObject);

				// Load Tower on Server, spreading to Clients
				ServerChangeScene (firstScene);
			}
			else Log.Info ("!Can't understand config file!");
		}

		#if UNITY_EDITOR
		[ContextMenu ("Register all NET prefabs")]
		public void RegisterNetPrefabs () 
		{
			spawnPrefabs.Clear ();
			string[] guids = UnityEditor.AssetDatabase.FindAssets ("t:GameObject");
			foreach (var g in guids)
			{
				string path = UnityEditor.AssetDatabase.GUIDToAssetPath (g);
				var go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject> (path);

				// Discard not networked objects & Player prefab
				if (go.name == "Player" || !go.GetComponent<NetworkIdentity> ()) continue; 
				spawnPrefabs.Add (go);
			}
		} 
		#endif
	}

	public partial class /* SERVER */ Net 
	{
		public static Player[] players;

		#region CALLBACKS
		public override void OnServerAddPlayer (NetworkConnection conn, short playerControllerId) 
		{
			// If it's the first time Player connects
			var player = GetPlayer (conn);
			if (!player)
			{
				// Create new persistent Player object
				player = Instantiate (playerPrefab).GetComponent<Player> ();
				player.PlayerSetup (conn);
				players[player.ID - 1] = player;
			}
			// Otherwise replace Player connection
			else player.Conn = conn;

			// Assign Player to new connection
			NetworkServer.AddPlayerForConnection (conn, player.gameObject, playerControllerId);
		}

		public override void OnServerDisconnect (NetworkConnection conn) 
		{
			// Use connectionId because address is not available on desconnection
			var player = players.Single (u=> u.Conn.connectionId == conn.connectionId);
			Log.Debug ("Player " + player.ID + " disconnected from server!");

			#warning Handle object destruction!
		}

		public override void OnServerSceneChanged (string sceneName) 
		{
			if (sceneName == "Tower")
			{
				// Pausing will make Players don't process input
				StartCoroutine ("WaitAllTowerPlayers");
				paused = true;
			}
			else
			{
				// Open courtain once a level finishes loading
				if (Courtain.net) Courtain.net.Open (true);
			}
		}
		#endregion

		#region HELPERS
		public static Player GetPlayer (NetworkConnection fromConn) 
		{
			return players.SingleOrDefault
				(p => p && p.IP == fromConn.address);
		}

		private IEnumerator WaitAllTowerPlayers () 
		{
			int playersReady; do
			{
				// Don't allow any kind of movement until all players are in
				playersReady = players.Count (p=> p && p.pawn is Hero);
				yield return null;
			}
			while (playersReady != PlayersNeeded);

			// Allow gameplay
			Courtain.net.Open (true);
			paused = false;
		}
		#endregion
	}

	public partial class /* CLIENT */ Net 
	{
		public static Player me;

		public void _OnClientSceneChanged (NetworkConnection conn) 
		{
			base.OnClientSceneChanged (conn);
			// Open courtain once a level finishes loading
			if (Courtain.net) Courtain.net.Open (true);
		}
	}
}
