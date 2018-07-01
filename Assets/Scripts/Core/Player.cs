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
		#region DATA
		public override string SharedName 
		{
			get { return "Player"; }
		}
		public string IP 
		{
			get { return connectionToClient.address; }
		}
		public int ID 
		{
			get { return connectionToClient.connectionId; }
		}

		[SyncVar] public Data data;
		#endregion

		#region CALLBACKS
		[ClientCallback]
		protected override void OnAuthoritySet () 
		{
			print ("Local Player set");
			Net.me = this;
		}

		protected override void OnAwake () 
		{
			DontDestroyOnLoad (this);
		}
		#endregion

		#region HELPERS
		public void SceneReady () 
		{
			if (Net.networkSceneName == "Selection" && data.selector.IsEmpty ()) 
			{
				var selector = FindObjectsOfType<Selector> ()[ID - 1];
				selector.id.AssignClientAuthority (connectionToClient);
				data.selector = selector.netId;
				selector.UpdateName ();
			}
		}
		#endregion

		[Serializable]
		public struct Data 
		{
			public NetworkInstanceId hero;
			public NetworkInstanceId selector;
		}
	}
}
