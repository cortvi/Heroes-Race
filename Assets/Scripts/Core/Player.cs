using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;

namespace HeroesRace 
{
	// This is the "Local players" class
	public class Player : NetBehaviour 
	{
		public override string SharedName 
		{
			get { return "Player"; }
		}

		#region CALLBACKS
		[ClientCallback]
		protected override void OnAuthoritySet () 
		{
			print ("Local Player set!");
//			Net.me = this;
		}

		protected override void OnAwake () 
		{
			DontDestroyOnLoad (this);
		}
		#endregion
	}

	/* This class holds persistent data for each Player
	 * when they connect for the first time. This way
	 * they disconnect and re-connect, authority will 
	 * restored for all owned objects. */
	public class User 
	{
		#region DATA + CTOR
		public readonly int ID;
		public string IP 
		{
			get { return Conn.address; }
		}

		public NetworkConnection Conn 
		{
			get { return Player.connectionToClient; }
		}
		public Player Player { get; private set; }
		public bool ready;

		public User (Player player) 
		{
			Player = player;
			ID = player.connectionToClient.connectionId;
		}
		#endregion

		public void AssignPlayer (Player player) 
		{
			Player = player;
			// Here I should re-authorize objects
		}
	}
}
