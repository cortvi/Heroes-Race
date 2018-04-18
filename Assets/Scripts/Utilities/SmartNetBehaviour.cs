using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public abstract class NetBehaviour : NetworkBehaviour 
{
	#region DATA
	/// When spawned, if local authority is set on 
	private static Dictionary<Type, NetBehaviour> localInstances;

	/// Network-shared name
	[SyncVar] public string netName;

	/// True if object can be controlled by
	/// the specific client it's checked on
	public bool isLocal;
	#endregion

	#region LOCAL AUTHORITY
	/// When spawned on the net, objects should be marked as local for
	/// each specific player in order for them to 
	[TargetRpc] public void Target_SetLocal (NetworkConnection target) 
	{
		/// Mark as local
		isLocal = true;

		/// Add to the dictionary
		var type = GetType ();
		if (localInstances.ContainsKey(type))
		{
			if (Debug.isDebugBuild)
				Debug.LogWarning ("Already exists an instance for type "+type+"!");
			return;
		}
		else localInstances.Add (type, this);

		/// Callback
		OnSetLocal ();
	}

	/// Client-side custom callback
	/// when object is marked as local
	[Client] protected virtual void OnSetLocal () 
	{
		name = name.Remove (0, 8);
	}

	protected virtual void Start () 
	{
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
