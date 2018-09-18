using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	// Wrapper class for networked objects
	public abstract class NetBehaviour : NetworkBehaviour 
	{
		public string SharedName { get; private set; }
		internal NetworkIdentity id;

		#region CALLBACKS
		// ——— Start wrapper ———
		protected virtual void OnStart () { }
		protected virtual void OnClientStart () { }
		protected virtual void OnServerStart () { }
		public void Start () 
		{
			OnStart ();
			if (NetworkClient.active) OnClientStart ();
			else
			if (NetworkServer.active) OnServerStart ();
		}

		// ——— Authority wrapper ———
		protected virtual void OnClientAuthority () { } 
		protected virtual void OnServerAuthority () { }
		public sealed override void OnStartAuthority () 
		{
			if (NetworkClient.active) OnClientAuthority ();
			else
			if (NetworkServer.active) OnServerAuthority ();
			UpdateName ();
		}

		// ——— Awake wrapper ———
		protected virtual void OnAwake () { }
		protected virtual void OnClientAwake () { }
		protected virtual void OnServerAwake () { }
		public void Awake () 
		{
			id = GetComponent<NetworkIdentity> ();
			SharedName = name;
			UpdateName ();

			OnAwake ();
			if (NetworkClient.active) OnClientAwake ();
			else
			if (NetworkServer.active) OnServerAwake ();
		}
		#endregion

		internal void UpdateName () 
		{
			string name = SharedName;
			if (NetworkClient.active)
			{
				if (hasAuthority) name = name.Insert (0, "[OWN] ");
				else			  name = name.Insert (0, "[OTHER] ");
			}
			else
			if (NetworkServer.active)
			{
				if (!id.serverOnly)
				{
					var o = id.clientAuthorityOwner;
					if (o != null) 
					{
						int id = Net.users.Find (u=> u.IP == o.address).ID;
						name = name.Insert (0, "CLIENT] ");
						name = name.Insert (0, "["+id+":");
					}
					else name = name.Insert (0, "[-CLIENT-] ");
				}
				else name = name.Insert (0, "[SERVER-ONLY] ");
			}
			// Show on inspector
			this.name = name;
		}
	} 
}
