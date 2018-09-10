using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.IO;

namespace HeroesRace 
{
	public partial class /* COMMON */ Net : NetworkManager 
	{
		public static Net worker;
		public static bool isClient;
		public static bool isServer;
		public static int UsersNeeded { get; private set; }

		[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void EntryPoint () 
		{
			worker = Extensions.SpawnSingleton<Net> ("Networker");
			Log.logLevel = Log.LogType.DeepDebug;

			// Read config:
			string[] config = File.ReadAllLines (Application.streamingAssetsPath + "/config.txt");
			if (config[0] == "client") 
			{
				isClient = true;
				worker.networkAddress = config[1];
				worker.StartClient ();
				Log.LowDebug ("This mahcine is now a client");
			}
			else
			if (config[0] == "server") 
			{
				isServer = true;
				worker.StartServer ();
				Log.LowDebug ("This mahcine is now the server");

				UsersNeeded = int.Parse (config[1]);
				users = new List<User> (UsersNeeded);
			}
			else Log.Info ("Can't understand config file!");
		}
	}

	public partial class /* SERVER */ Net 
	{
		public static List<User> users;
		public int UsersReady 
		{
			get { return users.Count (u => u.Conn.isReady); }
		}

		#region CALLBACKS
		public override void OnServerAddPlayer (NetworkConnection conn, short playerControllerId) 
		{
			// Spawned Player GO depends on scene,
			// for now testing not spawning at all
		}

		public override void OnServerDisconnect (NetworkConnection conn) 
		{
			// Set user un-ready
			var user = users.Find (u => u.Conn.connectionId == conn.connectionId);
			Log.Debug ("Player " + user.ID + " disconnected from server!");

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

	public partial class /* CLIENT */ Net 
	{
		public static Player me;

		/* Commented out for now because I just dont know TBH
		public override void OnClientConnect (NetworkConnection conn) 
		{
			Log.LowDebug ("Connected to Server, creating player!");

			ClientScene.Ready (conn);
			ClientScene.AddPlayer (conn, 0);
//			base.OnClientConnect (conn);
		}*/


		public override void OnClientDisconnect (NetworkConnection conn) 
		{
			#warning LAS PUTAS DESCONEXIONES SINGUEN DANDO POR CULO (se re-conecta automaticamente)
//			base.OnClientDisconnect (conn);
			// was this causing automatic re-connecting?
		}
	}
}
