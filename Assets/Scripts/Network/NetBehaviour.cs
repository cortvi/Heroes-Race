using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
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
			if (Net.isClient) OnClientStart ();
			else
			if (Net.isServer) OnServerStart ();
			OnStart ();
		}

		// ——— Awake wrapper ———
		protected virtual void OnClientAwake () { }
		protected virtual void OnServerAwake () { }
		protected virtual void OnAwake () { }
		public void Awake () 
		{
			name = SharedName = name.Replace ("(Clone)", "");
			if (Net.isClient) 
			{
				// No physic logic on Client!
				var colliders = GetComponentsInChildren<Collider> ();
				foreach (var c in colliders) Destroy (c);

				OnClientAwake ();
			}
			else if (Net.isServer) OnServerAwake ();
			OnAwake ();
		}
		#endregion
	}
}
