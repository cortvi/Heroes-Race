using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.IO;

namespace HeroesRace 
{
	// Common
	public partial class Net : NetworkManager 
	{
		public const int UsersNeeded = 1;

		public static Net worker;
		public static bool isServer;
		public static bool isClient;

		[RuntimeInitializeOnLoadMethod 
		(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void InitizalizeSingleton () 
		{
			// Creates a persistent Net-worker no matter the scene
			worker = Extensions.SpawnSingleton<Net> ("Networker");

			// Level of logging:
			Log.logLevel = Log.LogType.DeepDebug;

			// Read config:
			string[] config = File.ReadAllLines (Application.streamingAssetsPath + "config.txt");
			if (config[0] == "client")
			{
				isClient = true;
				worker.networkAddress = config[1];
				worker.StartClient ();
			}
			else
			{
				isServer = true;
				users = new List<User> (3);
			}
		}
	}

	// Server
	public partial class Net 
	{
		public static List<User> users;

		public int UsersReady 
		{
			get { return users.Count (u => u.ready); }
		}

		#region CALLBACKS
		public override void OnServerAddPlayer (NetworkConnection conn, short playerControllerId) 
		{
			// Spawn Player object over the net
			var player = Instantiate (playerPrefab).GetComponent<Player> ();
			NetworkServer.AddPlayerForConnection (conn, player.gameObject, playerControllerId);

			// Assign Player to the User
			var user = users.Find (u => u.IP == conn.address);
			user.AssignPlayer (player);
			user.ready = true;
		}

		public override void OnServerReady (NetworkConnection conn) 
		{
			// If it's the first time the Player connects
			var user = users.FirstOrDefault (u => u.IP == conn.address);
			if (user == null)
			{
				// Create a new persistent Player object
				Log.LowDebug ("Creating new persistent User");
				user = new User (conn);
				users.Add (user);
			}
			// Set User are ready!
			else user.ready = true;
			NetworkServer.SetClientReady (conn);
		}
		public override void ServerChangeScene (string newSceneName) 
		{
			// Set all users un-ready
			users.ForEach (u => u.ready = false);
			base.ServerChangeScene (newSceneName);

			// Wait until all players are ready
			StartCoroutine (WaitUsers ());
		}

		public override void OnServerDisconnect (NetworkConnection conn) 
		{
			// Set user un-ready
			var user = users.Find (u => u.Conn.connectionId == conn.connectionId);
			Log.Debug ("Player " + user.ID + " disconnected from server!");
			user.ready = false;

			//TOD=> Handle object destruction

		}
		#endregion

		#region HELPERS
		private IEnumerator WaitUsers () 
		{
			// Wait until Users report as ready
			while (UsersReady != UsersNeeded) 
				yield return null;

			Log.LowDebug ("Notifying users of scene change!");
			users.ForEach (u => u.SceneReady ());
		}
		#endregion
	}

	// Client
	public partial class Net 
	{
		public static Player me;

		public override void OnClientConnect (NetworkConnection conn) 
		{
			Log.LowDebug ("Connected to Server, creating player!");

			ClientScene.Ready (conn);
			ClientScene.AddPlayer (conn, 0);
//			base.OnClientConnect (conn);
		}
		public override void OnClientDisconnect (NetworkConnection conn) 
		{
			#warning LAS PUTAS DESCONEXIONES SINGUEN DANDO POR CULO (se re-conecta automaticamente)
//			base.OnClientDisconnect (conn);
			// was this causing automatic re-connecting?
		}

		public override void OnClientSceneChanged (NetworkConnection conn) 
		{
			Log.LowDebug ("Scene changed, making connection ready!");
			ClientScene.Ready (conn);
		}
	}
}
