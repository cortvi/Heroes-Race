using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// Special class for objects that should be
// controlled by specific clients over the network
public abstract class NetBehaviour : NetworkBehaviour 
{
	#region DATA
	internal NetworkIdentity id;

	// Network-shared name
	[SyncVar (hook = "UpdateName")]
	private string netName;

	// List of owned objects by type
	private static Dictionary<Type, NetBehaviour> ownInstances = new Dictionary<Type, NetBehaviour> ();
	#endregion

	#region CALLBACKS
	public void Awake () 
	{
		id = GetComponent<NetworkIdentity> ();
		OnAwake ();
	}
	protected virtual void OnAwake () { }

	public sealed override void OnStartAuthority () 
	{
		// Register as own
		if (isClient) AddToDictionary ();
		OnSetAuthority ();
	}
	protected virtual void OnSetAuthority () { }
	#endregion

	#region HELPERS
	[Server]
	public void SetName (string name) 
	{
		UpdateName (name);
	}
	private void UpdateName (string name) 
	{
		string displayName = name;
		if (isClient) 
		{
			if (hasAuthority)	displayName = displayName.Insert (0, "[OWN] ");
			else				displayName = displayName.Insert (0, "[OTHER] ");
		}
		else
		if (isServer) 
		{
			print (id);
			if (!id.serverOnly)
			{
				displayName = displayName.Insert (0, "[CLIENT] ");

				var o = id.clientAuthorityOwner;
				if (o != null) displayName = displayName.Insert (0, "["+o.connectionId+"]");
			}
			else displayName = displayName.Insert (0, "[SERVER-ONLY] ");
		}

		// Show on inspector
		this.name = displayName;
		netName = name;
	}

	// Returns the scene object that has local Player authority of given type
	[Client] public static T GetOwn<T> () where T : NetBehaviour
	{
		NetBehaviour own;
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
	#endregion
}
