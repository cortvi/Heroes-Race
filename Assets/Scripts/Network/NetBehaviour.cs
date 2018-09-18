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
		[ClientCallback] protected virtual void OnClientStart () { }
		[ServerCallback] protected virtual void OnServerStart () { }
		public void Start () 
		{
			OnStart ();
			OnClientStart ();
			OnServerStart ();
			UpdateName ();
		}

		// ——— Authority wrapper ———
		[ClientCallback] protected virtual void OnClientAuthority () { } 
		[ServerCallback] protected virtual void OnServerAuthority () { }
		public sealed override void OnStartAuthority () 
		{
			OnClientAuthority ();
			OnServerAuthority ();
			UpdateName ();
		}

		// ——— Awake wrapper ———
		protected virtual void OnAwake () { }
		[ClientCallback] protected virtual void OnClientAwake () { }
		[ServerCallback] protected virtual void OnServerAwake () { }
		public void Awake () 
		{
			id = GetComponent<NetworkIdentity> ();
			SharedName = name;

			OnAwake ();
			OnClientAwake ();
			OnServerAwake ();
		}
		#endregion

		internal void UpdateName () 
		{
			string name = SharedName;
			if (id.isClient)
			{
				if (hasAuthority) name = name.Insert (0, "[OWN] ");
				else			  name = name.Insert (0, "[OTHER] ");
			}
			else
			if (id.isServer)
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
