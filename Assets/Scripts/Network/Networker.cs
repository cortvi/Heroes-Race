using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace HeroesRace 
{
	public class Networker : NetworkManager
	{
		#region DATA
		public static Networker i;
		public static List<Game> players;

		public static bool DedicatedServer
		{
			get { return (NetworkServer.active && !NetworkServer.localClientActive); }
		}
		public static bool DedicatedClient
		{
			get { return (NetworkClient.active && !NetworkServer.localClientActive); }
		}
		public static bool IsHost
		{
			get { return NetworkServer.localClientActive; }
		}
		#endregion

		#region SERVER
		public override void OnServerAddPlayer (NetworkConnection conn, short playerControllerId)
		{

			// Spawn player object over the net
			var player = Instantiate (playerPrefab).GetComponent<Game> ();
			NetworkServer.AddPlayerForConnection (conn, player.gameObject, playerControllerId);
			players.Add (player);

			#region SCENE BEHAVIOUR
			// Behaviour based on what scene we start at
			string scene = SceneManager.GetActiveScene ().name;
			if (scene == "Menu") 
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
				var asignedHero = (Game.Heroes)conn.connectionId;
				player.SpawnHero (asignedHero);
			}
			#endregion
		}
		#endregion

		#region CALLBACKS
		public override void ServerChangeScene (string newSceneName) 
		{
			// Read all the selected heroes
			Game.Heroes[] heroes = new Game.Heroes[3];
			var selectors = FindObjectsOfType<Selector> ();
			for (int i=0; i!=3; i++) heroes[i] = selectors[i].ReadHero ();

			// Actually change scene
			base.ServerChangeScene (newSceneName);

			// Spawn the heroes
			for (int i=0; i!=players.Count; i++)
				players[i].SpawnHero (heroes[i]);
		}

		// Creates this object on every scene no matter where
		[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void InitizalizeSingleton ()
		{
			i = Extensions.SpawnSingleton<Networker> ();
			players = new List<Game> (3);
		}
		#endregion
	} 
}
