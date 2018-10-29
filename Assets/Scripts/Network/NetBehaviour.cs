using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	[DisallowMultipleComponent]
	// Wrapper class for networked objects
	public abstract class NetBehaviour : NetworkBehaviour 
	{
		public string SharedName { get; private set; }

		#region CALLBACKS
		// ——— Start wrapper ———
		protected virtual void OnClientStart () { }
		protected virtual void OnServerStart () { }
		protected virtual void OnStart () { }
		public void Start () 
		{
			if (NetworkClient.active) OnClientStart ();
			else
			if (NetworkServer.active) OnServerStart ();
			OnStart ();
		}

		// ——— Awake wrapper ———
		protected virtual void OnClientAwake () { }
		protected virtual void OnServerAwake () { }
		protected virtual void OnAwake () { }
		public void Awake () 
		{
			name = SharedName = name.Replace ("(Clone)", "");
			if (NetworkClient.active) OnClientAwake ();
			else
			if (NetworkServer.active) OnServerAwake ();
			OnAwake ();
		}
		#endregion
	}
}
