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
		internal bool isPawn;

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
			UpdateName ();
		}

		// ——— Awake wrapper ———
		protected virtual void OnAwake () { }
		protected virtual void OnClientAwake () { }
		protected virtual void OnServerAwake () { }
		public void Awake () 
		{
			OnAwake ();
			if (NetworkClient.active) OnClientAwake ();
			else
			if (NetworkServer.active) OnServerAwake ();
			SharedName = name.Replace ("(Clone)", "");
		}
		#endregion

		#region NETWORK
		public virtual void OnBecomePawn ()  { }

		internal void UpdateName () 
		{
			string name = SharedName;
			if (NetworkClient.active)
			{
				if (isPawn || isLocalPlayer) name = name.Insert (0, "[OWN] ");
				else name = name.Insert (0, "[OTHER] ");
			}
			else
			if (NetworkServer.active)
			{
				if (!GetComponent<NetworkIdentity> ().serverOnly)
				{
					var owner = Net.users.FirstOrDefault (u=> u.player.pawn == this || u.player == this);
					if (owner != null) 
					{
						name = name.Insert (0, "CLIENT] ");
						name = name.Insert (0, "[" + owner.ID + ":");
					}
					else name = name.Insert (0, "[-CLIENT-] ");
				}
				else name = name.Insert (0, "[SERVER-ONLY] ");
			}
			// Show on inspector
			this.name = name;
		}
		#endregion
	} 
}
