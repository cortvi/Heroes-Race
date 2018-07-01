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
			if (Net.isServer)
				SceneManager.sceneLoaded += SceneReady;
		}
		private void OnDestroy () 
		{
			if (Net.isServer)
				SceneManager.sceneLoaded -= SceneReady;
		}
		#endregion

		#region HELPERS
		private void SceneReady (Scene scene, LoadSceneMode mode) 
		{
			// Made coroutine to ensure conditions are met
			StartCoroutine (SceneLogic ());
		}
		IEnumerator SceneLogic () 
		{
			if (Net.networkSceneName == "Selection" && data.selector.IsEmpty ()) 
			{
				Selector[] selectors;
				do
				{
					selectors = FindObjectsOfType<Selector> ();
					yield return null;
				}
				while (selectors == null || selectors.Length == 0);

				 // Once selectors are enabled and accesible
				var selector = selectors[ID - 1];
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
