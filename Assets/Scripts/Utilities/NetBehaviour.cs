using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// Special class for objects that should be
/// controlled by specific clients over the network
public abstract class NetBehaviour : NetworkBehaviour 
{
	#region DATA
	/// When spawned, if local authority is set on 
	private static Dictionary<Type, NetBehaviour> localInstances
			 = new Dictionary<Type, NetBehaviour> ();

	/// True if object can be controlled by
	/// the specific client it's checked on
	public bool isLocal;

	/// Network-shared name
	[SyncVar] internal string netName;
	#endregion

	#region MyRegion
	/// When spawned on the net, objects should be marked as local for
	/// each specific player in order for them to 
	[TargetRpc] public void Target_SetLocal (NetworkConnection target)
	{
		/// Mark as local
		isLocal = true;

		/// Add to the dictionary
		var type = GetType ();
		if (localInstances.ContainsKey (type))
		{
			if (Debug.isDebugBuild)
				Debug.LogWarning ("Already exists an instance for type " + type + "!");
			return;
		}
		else localInstances.Add (type, this);

		/// Callback
		OnSetLocal ();
	} 
	#endregion

	#region CALLBACKS
	/// Client-side custom callback
	/// when object is marked as local
	[Client] protected virtual void OnSetLocal () 
	{

	}

	protected virtual void Start () 
	{
		var id = GetComponent<NetworkIdentity> ();
		print (name);
		if (id.clientAuthorityOwner != null)
			print ("Owner is:" + id.clientAuthorityOwner.connectionId);
		if (connectionToClient != null)
			print ("My coon to client is:" + connectionToClient.connectionId);
		if (connectionToServer != null)
			print ("My conn to server is:" + connectionToServer.connectionId);
		name = netName.Insert (0, "[OTHER] ");
	} 
	#endregion

	#region HELPERS
	/// Returns the scene object that has local Player authority of given type
	[Client] public static T GetLocal<T> () where T : NetBehaviour 
	{
		NetBehaviour local;
		if (!localInstances.TryGetValue (typeof (T), out local))
		{
			if (Debug.isDebugBuild)
				Debug.LogWarning ("No local instance for method " + typeof (T) + " found.");
			return null;
		}
		else return local as T;
	}
	#endregion
}
