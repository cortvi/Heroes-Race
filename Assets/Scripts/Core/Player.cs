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

		[SyncVar] public Data data;
		#endregion

		#region CALLBACKS
		protected override void OnAwake () 
		{
			DontDestroyOnLoad (this);
			if (Net.isClient && isLocalPlayer)
			{
				print ("Local player set");
				Net.me = this;
			}
		}
		#endregion

		#region HELPERS
		public void SceneReady () 
		{
			if (Net.networkSceneName == "Selection" && !data.selector) 
			{
				var selector = FindObjectsOfType<Selector> ()[data.ID - 1];
				selector.id.AssignClientAuthority (connectionToClient);
				data.selector = selector.gameObject;
			}
		}

		public void SetData (int id, string ip) 
		{
			data.ID = id;
			data.IP = ip;
		}
		#endregion

		[Serializable]
		public struct Data 
		{
			public int ID;
			public string IP;
			public GameObject hero;
			public GameObject selector;
		}
	}
}
