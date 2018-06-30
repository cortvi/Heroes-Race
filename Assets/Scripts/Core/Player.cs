using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace HeroesRace 
{
	// This is the "Local players" class
	public class Player : NetBehaviour 
	{
		#region DATA
		public override string SharedName 
		{
			get { return "Player"; }
		}
		#endregion

		#region CALLBACKS
		protected override void OnAwake () 
		{
			DontDestroyOnLoad (this);
		}
		#endregion
	}

	/* This class contains the local objects authored
	 * by each player. Even if they disconnect, objects remain and
	 * their local authority will be re-set when connected again.
	 *
	 * Users are persistent, since when created, IP address is stored,
	 * so that when reconnected same User is assigned.*/
	public class User 
	{
		#region DATA + CTOR
		public readonly int ID;
		public readonly string IP;

		public Player Player { get; private set; }
		public Selector Selector { get; private set; }
		public Hero Hero { get; private set; }

		public User (Player player, int id, string ip) 
		{
			ID = id;
			IP = ip;
			Player = player;
		} 
		#endregion

		#region UTILS
		public void SceneChange (string scene) 
		{
			if (scene == "Selection") 
			{
				Selector = GameObject.Find ("[CLIENT] Selector_" + ID).GetComponent<Selector> ();
				Selector.id.AssignClientAuthority (Player.connectionToClient);
			}
		}

		public void SpawnHero (Heroes hero) 
		{
			/*
			// Instantiate Hero object
			var go = Object.Instantiate (Resources.Load<Hero> ("Prefabs/Heroes/" + hero.ToString ()));
			hero.identity = playingAs;

			// Propagate over the Net
			NetworkServer.SpawnWithClientAuthority (hero.gameObject, connectionToClient);
			*/
		}
		#endregion
	}
}
