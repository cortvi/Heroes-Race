using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// Ensures correct behaviour for all objects
/// that should work both in offline and online
public abstract class AdaptativeBehaviour
#if (NETWORK)
/* Playing on-line  */					: NetworkBehaviour
#else
/* Playing off-line */					: MonoBehaviour
#endif

/// Class body
{
	#region TESTING STILL
	public enum Network 
	{
//		Offline,
		Client,
		AuthorizedCliend,
		Server
	}
	#endregion

	#region DATA
	public Network runOn;

	#if (!NETWORK)
	/// These simulate NetworkBehaviour properties
	[NonSerialized] protected bool isClient;
	[NonSerialized] protected bool isServer;
	[NonSerialized] protected bool hasAuthority; 
	#endif
	#endregion

	#region CALLBACKS

	#region TESTING STILL
	private void Update () 
	{

	}
	protected virtual void OnUpdate () { }

	public void Call (Action call, Network onActor)
	{

	}
	/*
	public T Call<P1, T> ( Func<P1, T> call, P1 arg, NetworkActor onActor ) 
	{
		return call.Invoke ( arg );
	}
	*/ 
	#endregion

	protected virtual void Awake () 
	{
		#if (!NETWORK)
		/// Emulate NetworkBehaviour properties
		isServer = (runOn == Network.Server);
		isClient = (runOn == Network.Client) || (runOn == Network.AuthorizedCliend);
		hasAuthority = (runOn == Network.Server) || (runOn == Network.AuthorizedCliend);
		#endif
	}
	#endregion
}
