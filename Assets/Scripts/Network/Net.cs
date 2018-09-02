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
		public static Player me;

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

			// Assign Player to the User
			var user = users.Find (u => u.IP == conn.address);
			user.AssignPlayer (player);
			user.ready = true;
		}

		public override void ServerChangeScene (string newSceneName) 
		{
			// Set all users un-ready
			users.ForEach (u => u.ready = false);
			base.ServerChangeScene (newSceneName);

			// Wait until all players are ready
			StartCoroutine (WaitUsers ());
		}
		public override void OnServerReady (NetworkConnection conn) 
		{
			// If it's the first time the Player connects
			var user = users.FirstOrDefault (u => u.IP == conn.address);
			if (user == null)
			{
				// Create a new persistent Player object
				print ("Creating new persistent User");
				user = new User (conn);
				users.Add (user);
			}
			// Set User are ready!
			else user.ready = true;
			NetworkServer.SetClientReady (conn);
		}

		public override void OnServerDisconnect (NetworkConnection conn) 
		{
			// Set user un-ready
			var user = users.Find (u => u.Conn.connectionId == conn.connectionId);
			print ("Player " + user.ID + " disconnected from server!");
			user.ready = false;

			//TOD=> Handle object destruction

		}

		public override void OnStartServer () 
		{
			isServer = true;
		}
		public override void OnStopServer () 
		{
			isServer = false;
			users.Clear ();
		}
		#endregion

		#region CLIENT
		public override void OnClientSceneChanged (NetworkConnection conn) 
		{
			ClientScene.Ready (conn);
		}

		public override void OnClientConnect (NetworkConnection conn) 
		{
			// Default implementation (?)
			ClientScene.Ready (conn);
			ClientScene.AddPlayer (0);
		}
		public override void OnClientDisconnect (NetworkConnection conn) 
		{
			#warning LAS PUTAS DESCONEXIONES SINGUEN DANDO POR CULO
//			base.OnClientDisconnect (conn);
			// was this causing automatic re-connecting?
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

		#region HELPERS
		private IEnumerator WaitUsers () 
		{
			// Wait until Users report as ready
			while (ClientsReady != UsersNeeded)
				yield return null;

			print ("Notifying users of scene change!");
			users.ForEach (u => u.SceneReady ());
		}

		[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void InitizalizeSingleton () 
		{
			// Creates a persistent Net-worker no matter the scene
			worker = Extensions.SpawnSingleton<Net> ("Networker");
			users = new List<User> (3);
		}
		#endregion
	} 
}
