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
			Application.targetFrameRate = 60;

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
				UsersNeeded = int.Parse (config[1]);
				users = new List<User> (UsersNeeded);

				worker.StartServer ();
				Log.LowDebug ("This mahcine is now the server");
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
			var user = users.Find (u => u.IP == conn.address);
			user.player = Instantiate (playerPrefab).GetComponent<Player> ();
			NetworkServer.AddPlayerForConnection (conn, user.player.gameObject, playerControllerId);

			// Assign correct Pawn to player
			if (networkSceneName == "Selection") 
			{
				var check = new Func<Selector, bool> (s=> s.SharedName == "Selector_" + user.ID);
				var selector = FindObjectsOfType<Selector> ().First (check);

				user.player.pawn = selector;
				selector.UpdateName ();
			}
			else
			if (networkSceneName == "Tower") 
			{
				// If bypassing selection menu
				if (user.playingAs == Heroes.NONE)
					user.playingAs = (Heroes)user.ID;

				// Spawn Heroe & set up its Driver
				var hero = Instantiate(Resources.Load<Hero> ("Prefabs/Heroes/" + user.playingAs));
				hero.driver = Instantiate (Resources.Load<Driver> ("Prefabs/Character_Driver"));
				hero.driver.name = user.playingAs + "_Driver";
				hero.driver.owner = hero;

				NetworkServer.Spawn (hero.gameObject);
				user.player.pawn = hero;

				// Add a Hero camera for testing in the server!
				hero.OnBecomePawn ();
			}
			// Notify Client of new Pawn
			user.player.Target_SetPawn (conn, user.player.pawn.gameObject);
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

			#warning Handle object destruction!
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
