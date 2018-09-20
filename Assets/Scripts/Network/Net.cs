using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.IO;
using System;

namespace HeroesRace 
{
	public partial class /* COMMON */ Net : NetworkManager 
	{
		public static Net worker;
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
				worker.networkAddress = config[1].Trim ();
				worker.StartClient ();
				Log.LowDebug ("This mahcine is now a client");
			}
			else
			if (config[0] == "server") 
			{
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
		public int UsersReadyCount 
		{
			get { return users.Count (u => u.Conn.isReady); }
		}

		#region CALLBACKS
		public override void OnServerAddPlayer (NetworkConnection conn, short playerControllerId) 
		{
			#region esto ira en otro sitio (SceneChange seguramente)
			/*
	GameObject player = null;
	var user = users.Find (u => u.IP == conn.address);
	if (networkSceneName == "Selection") 
	{
		var check = new Func<Selector, bool> (s=> s.SharedName == "Selector_" + user.ID);
		player = FindObjectsOfType<Selector> ().First (check).gameObject;
		// Is the selector gonna replicate over the network??
	}
	else
	if (networkSceneName == "Tower") 
	{
		// If bypassing selection menu
		if (user.playingAs == Heroes.NONE)
			user.playingAs = (Heroes) user.ID;

		// Spawn Heroes and use it as the player
		print ("Creating " + user.playingAs);
		var prefab = Resources.Load ("Perfabs/Heroes/" + user.playingAs);
		player = Instantiate (prefab) as GameObject;
		player.GetComponent<Hero> ().owner = user;
	}
	*/
			#endregion

			// Player objects are destroyed between scenes, so no need to call Replace
			NetworkServer.AddPlayerForConnection (conn, player, playerControllerId);
			player.GetComponent<NetBehaviour> ().UpdateName ();
		}

		public override void OnServerConnect (NetworkConnection conn) 
		{
			// Check if it's the first time the player connects
			var user = users.FirstOrDefault (u=> u.IP == conn.address);
			if (user == null) users.Add (new User (conn));
			else user.Conn = conn;

			base.OnServerConnect (conn);
		}
		public override void OnServerDisconnect (NetworkConnection conn) 
		{
			var user = users.Find (u=> u.Conn.connectionId == conn.connectionId);
			Log.Debug ("Player " + user.ID + " disconnected from server!");

			//TOD=> Handle object destruction

		}
		#endregion
	}

	public partial class /* CLIENT */ Net 
	{
		public override void OnClientConnect (NetworkConnection conn) 
		{
//			ClientScene.Ready (conn);
//			ClientScene.AddPlayer (conn, 0);
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
