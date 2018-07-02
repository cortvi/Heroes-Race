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
			Net.me = this;
		}

		protected override void OnAwake () 
		{
			DontDestroyOnLoad (this);
		}
		#endregion
	}
}
