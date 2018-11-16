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
		public static Net worker { get; private set; }
		public static int PlayersNeeded { get; private set; }
		public static bool IsServer { get; private set; }
		public static bool IsClient { get; private set; }
		public static bool paused { get; private set; }

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

			// Spawn whole-session courtain
			Instantiate (Resources.Load ("Prefabs/Courtain")).name = "Courtain";

			// Read config file
			string[] config = File.ReadAllLines (Application.streamingAssetsPath + "/config.txt");
			worker.StartCoroutine (worker.Config (config));
		}

		private IEnumerator Config (string[] config) 
		{
			// Go to a neutral scene to sync Server & Client
			if (SM.GetActiveScene ().name != "!Zero")
				yield return SM.LoadSceneAsync ("!Zero");

			// Start Client
			if (config[0] == "client") InitClient (config[1].Trim ());
			else
			// Start Server
			if (config[0] == "server") 
			{
				// Initialize to expect given Players
				InitServer (int.Parse (config[1]));

				// Wait until players are in
				Courtain.SetText ("Esperando jugadores...");
				while (!PlayersReady ()) yield return null;

				// Load Tower on Server, spreading to Clients
				Courtain.SetText ("Generando mapa...");
				ServerChangeScene (firstScene);
			}
			else Debug.LogError ("!Can't understand config file!");
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
			if (player == null) 
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
				// Paused until all Players arrive
				StartCoroutine ("WaitAllTowerPlayers");
			}
		}

		public override void OnServerError (NetworkConnection conn, int errorCode) 
		{
			print ("lmao");
		}
		#endregion

		#region UTILS
		public static Player GetPlayer (NetworkConnection fromConn)
		{
			// Returns the Player registered with given IP address 
			return players.SingleOrDefault (p => p && p.IP == fromConn.address);
		}

		public static bool PlayersReady (Func<Player, bool> check = null)
		{
			int basicCount = players.Count (p => p && p.Conn.isReady);
			int checkCount = (check != null) ? players.Count (check) : PlayersNeeded;

			return (basicCount == PlayersNeeded && checkCount == PlayersNeeded);
		} 
		#endregion

		#region HELPERS
		private void InitServer (int playersNeeded) 
		{
			PlayersNeeded = playersNeeded;
			players = new Player[playersNeeded];

			StartServer ();
			IsServer = true;
			paused = true;
			Log.LowDebug ("This mahcine is now the server");
		}

		private IEnumerator WaitAllTowerPlayers () 
		{
			// Don't allow any kind of movement until all players are in
			Courtain.SetText ("Esperando a jugadores...");
			while (!PlayersReady (p=> p.pawn is Hero)) yield return null;

			// Allow gameplay
			Courtain.Open (true);
			paused = false;
		}
		#endregion
	}

	public partial class /* CLIENT */ Net 
	{
		public static Player me;

		private void InitClient (string ipAddress) 
		{
			networkAddress = ipAddress;
			StartClient ();
			IsClient = true;
			Log.LowDebug ("This mahcine is now a client");
		}
	}
}
