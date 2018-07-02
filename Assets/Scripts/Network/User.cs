using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	/* This class holds persistent data for each Player
* when they connect for the first time. This way
* they disconnect and re-connect, authority will 
* restored for all owned objects. */
	public class User
	{
		#region DATA + CTOR
		public int ID { get; private set; }
		public string IP { get; private set; }

		public NetworkConnection Conn 
		{
			get { return Player.connectionToClient; }
		}
		public Player Player { get; private set; }
		public bool ready;

		public Heroes playingAs;

		public User (NetworkConnection firstConn) 
		{
			playingAs = Heroes.NONE;
			ID = firstConn.connectionId;
			IP = firstConn.address;
		}
		#endregion

		#region UTILS
		public void AssignPlayer (Player player) 
		{
			Player = player;
			// Here I should re-authorize objects

		}

		public void SceneReady () 
		{
			// Behaviour based on what scene we start at
			if (Net.networkSceneName == "Selection") 
			{
				Player.print ("Assigning authority to Selectors");
				var selector = UnityEngine.Object.FindObjectsOfType<Selector> ()[ID - 1];
				selector.id.AssignClientAuthority (Player.connectionToClient);
				selector.UpdateName ();
			}
			else
			if (Net.networkSceneName == "Tower") 
			{
				// If bypassing selection menu
				if (playingAs == Heroes.NONE)
					playingAs = (Heroes)ID;

				// Spawn hero
				var prefab = Resources.Load<Hero> ("Prefabs/Heroes/" + playingAs);
				var hero = Object.Instantiate (prefab);
				hero.identity = playingAs;

				// Propagate over the net and give it authority
				NetworkServer.SpawnWithClientAuthority (hero.gameObject, Player.gameObject);
			}
		}
		#endregion
	}
}

