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
		#region DATA
		// Network-shared name
		public abstract string SharedName { get; }

		internal NetworkIdentity id;
		#endregion

		#region CALLBACKS
		public void Start () 
		{
			OnStart ();
		}
		protected virtual void OnStart () { }

		public sealed override void OnStartAuthority () 
		{
			UpdateName ();
			OnAuthoritySet ();
		}
		protected virtual void OnAuthoritySet () { } 

		public void Awake () 
		{
			id = GetComponent<NetworkIdentity> ();
			UpdateName ();
			OnAwake ();
		}
		protected virtual void OnAwake () { } 
		#endregion

		#region HELPERS
		public void UpdateName () 
		{
			string name = SharedName;
			if (Net.isClient)
			{
				if (hasAuthority) name = name.Insert (0, "[OWN] ");
				else name = name.Insert (0, "[OTHER] ");
			}
			else
			if (Net.isServer)
			{
				if (!id.serverOnly)
				{
					var o = id.clientAuthorityOwner;
					if (o != null) 
					{
						name = name.Insert (0, "[CLIENT] ");
						name = name.Insert (0, "[" + o.connectionId + "]:");
					}
					else return;
				}
				else name = name.Insert (0, "[SERVER-ONLY] ");
			}
			// Show on inspector
			this.name = name;
		}
		#endregion
	} 
}
