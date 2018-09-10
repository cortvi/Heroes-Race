using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/* This class holds persistent data for each player
 * when they connect for the first time. This way when
 * they disconnect and re-connect, authority will be
 * restored for all owned objects. */
namespace HeroesRace 
{
	public class /* SERVER-ONLY */ User 
	{
		#region DATA + CTOR
		public readonly int ID;
		public string IP { get; private set; }
		public NetworkConnection Conn 
		{
			get { return Player.connectionToClient; }
		}

		public Player Player { get; set; }
		public Heroes playingAs;

		public User (NetworkConnection fromConn) 
		{
			IP = fromConn.address;
			ID = fromConn.connectionId;
			playingAs = Heroes.NONE;
		}
		#endregion

		#region UTILS
		// Behaviour based on what scene we're at
		public void SceneReady () 
		{
			if (Net.networkSceneName == "Selection") 
			{
				Log.Debug ("Assigning authority to Selectors");
				var selector = Object.FindObjectsOfType<Selector> ()[ID - 1];
				selector.id.AssignClientAuthority (Player.connectionToClient);
				selector.UpdateName ();
			}
			else
			if (Net.networkSceneName == "Tower") 
			{
				// If bypassing selection menu
				if (playingAs == Heroes.NONE)
					playingAs = (Heroes) ID;

				// Spawn hero over the net
				var prefab = Resources.Load ("Prefabs/Heroes/" + playingAs);
				NetworkServer.Spawn (Object.Instantiate (prefab) as GameObject);
			}
		}
		#endregion
	}
}

