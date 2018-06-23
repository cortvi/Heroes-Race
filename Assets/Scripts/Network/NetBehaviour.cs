using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	// Special class for objects that should be
	// controlled by specific clients over the network
	public abstract class NetBehaviour : NetworkBehaviour
	{
		#region DATA
		internal NetworkIdentity id;

		// Network-shared name
		public abstract string SharedName { get; }

		// List of owned objects by type
		private static Dictionary<Type, NetBehaviour> ownInstances = new Dictionary<Type, NetBehaviour> ();
		#endregion

		#region CALLBACKS
		public void Start () 
		{
			// Once authority is set
			UpdateName ();
			OnStart ();
		}
		protected virtual void OnStart () { }

		public sealed override void OnStartAuthority () 
		{
			UpdateName ();
			// Register as own on Client
			if (isClient) AddToDictionary ();
			OnAuthoritySet ();
		}
		protected virtual void OnAuthoritySet () { }

		public void Awake () 
		{
			id = GetComponent<NetworkIdentity> ();
			OnAwake ();
		}
		protected virtual void OnAwake () { }
		#endregion

		#region HELPERS
		[Client] public static T GetOwn<T> () where T : NetBehaviour
		{
			NetBehaviour own;
			// Returns the scene object that has local Player authority of given type
			if (!ownInstances.TryGetValue (typeof (T), out own))
			{
				if (Debug.isDebugBuild)
					Debug.LogWarning ("No own instance for type " + typeof (T) + " found.");
				return null;
			}
			else return own as T;
		}
		[Client] private void AddToDictionary ()
		{
			var type = GetType ();
			ownInstances.Add (type, this);
		}

		public void UpdateName () 
		{
			string name = SharedName;
			if (isClient)
			{
				if (hasAuthority) name = name.Insert (0, "[OWN] ");
				else name = name.Insert (0, "[OTHER] ");
			}
			else
			if (isServer)
			{
				if (!id.serverOnly)
				{
					name = name.Insert (0, "[CLIENT] ");

					var o = id.clientAuthorityOwner;
					if (o != null) name = name.Insert (0, "[" + o.connectionId + "]");
				}
				else name = name.Insert (0, "[SERVER-ONLY] ");
			}
			// Show on inspector
			this.name = name;
		}
		#endregion
	} 
}
