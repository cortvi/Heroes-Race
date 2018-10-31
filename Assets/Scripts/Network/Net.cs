using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace HeroesRace 
{
	public partial class /* COMMON */ Net : NetworkManager 
	{
		public static Net worker;
		public static int PlayersNeeded { get; private set; }
		public static bool isClient;
		public static bool isServer;

		[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void EntryPoint () 
		{
			// Initialize some basic aspects
			worker = Extensions.SpawnSingleton<Net> ("Networker");
			Log.logLevel = Log.LogType.DeepDebug;
			Application.targetFrameRate = 60;

			// Read config:
			string[] config = File.ReadAllLines (Application.streamingAssetsPath + "/config.txt");
			if (config[0] == "client") 
			{
				// Start Client from given address
				worker.networkAddress = config[1].Trim ();
				worker.StartClient ();
				Log.LowDebug ("This mahcine is now a client");
				isClient = true;
			}
			else
			if (config[0] == "server") 
			{
				// Start Server to expect given Players
				PlayersNeeded = int.Parse (config[1]);
				players = new Player[PlayersNeeded];

				worker.StartServer ();
				Log.LowDebug ("This mahcine is now the server");
				isServer = true;
			}
			else Log.Info ("Can't understand config file!");
		}
	}

	public partial class /* SERVER */ Net 
	{
		public static Player[] players;
		/*public int PlayerReadyCount 
		{
			get { return players.Count (u => u.Conn.isReady); }
		}*/

		#region CALLBACKS
		public override void OnServerAddPlayer (NetworkConnection conn, short playerControllerId) 
		{
			// Check if it's Player's first time
			var player = CheckPlayer (conn);

			// Create new Pawn for Player if none
			string scene = networkSceneName;
			if (player.pawn == null) 
			{
				if (scene == "Selection")
				{
					var check = new Func<Selector, bool> (s => s.SharedName == "Selector_" + player.ID);
					var selector = FindObjectsOfType<Selector> ().First (check);

					player.SetPawn (selector);
				}
				else
				if (scene == "Tower")
				{
					// If bypassing selection menu
					if (player.playingAs == Heroe.NONE)
						player.playingAs = (Heroe)player.ID;

					// Spawn Heroe & set up its Driver
					var hero = Instantiate (Resources.Load<Hero> ("Prefabs/Heroes/" + player.playingAs));
					hero.driver = Instantiate (Resources.Load<Driver> ("Prefabs/Character_Driver"));
					hero.driver.name = player.playingAs + "_Driver";
					hero.driver.owner = hero;

					NetworkServer.Spawn (hero.gameObject);
					player.SetPawn (hero);
				}
			}
		}

		public override void OnStartServer () 
		{
			// Ensures all current net objects are spawned
			networkSceneName = SceneManager.GetActiveScene ().name;
			StartCoroutine (ActiveCurrentScene ());
		}
		IEnumerator ActiveCurrentScene () 
		{
			var objs = FindObjectsOfType<NetworkIdentity> ();
			bool allSetUp = true;
			do
			{
				allSetUp = true;
				foreach (var o in objs)
				{
					// Ensure all objects has been
					// given a SceneID value by Unity
					if (o.sceneId.Value == 0) allSetUp = false;
				}
				yield return null;
			}
			while (!allSetUp);
			NetworkServer.SpawnObjects ();
			base.OnStartServer ();
		}

		public override void OnServerDisconnect (NetworkConnection conn) 
		{
			// Use connectionId because address is not available on desconnection
			var player = players.Single (u=> u.Conn.connectionId == conn.connectionId);
			Log.Debug ("Player " + player.ID + " disconnected from server!");

			#warning Handle object destruction!
		}
		#endregion

		#region HELPERS
		private Player CheckPlayer (NetworkConnection conn) 
		{
			var player = GetPlayer (conn);
			if (player == null)
			{
				// If it's the first time the Player connects
				player = Instantiate (playerPrefab).GetComponent<Player> ();
				player.PlayerSetup (conn);

				players[player.ID - 1] = player;
				NetworkServer.AddPlayerForConnection (conn, player.gameObject, 0);
			}
			// Replace Player connection to new one
			else player.Conn = conn;
			return player;
		}

		public static Player GetPlayer (NetworkConnection fromConn) 
		{
			return players.SingleOrDefault
				(p => p && p.IP == fromConn.address);
		} 
		#endregion
	}

	public partial class /* CLIENT */ Net 
	{
		public static Player me;

		public override void OnClientConnect (NetworkConnection conn) 
		{
			base.OnClientConnect (conn);
			Log.LowDebug ("Connected to Server!");
		}

		public override void OnClientDisconnect (NetworkConnection conn) 
		{
			#warning LAS PUTAS DESCONEXIONES SINGUEN DANDO POR CULO (se re-conecta automaticamente)
			// was this causing automatic re-connecting?
			base.OnClientDisconnect (conn);
		}
	}
}
