using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// Ensures correct behaviour for all objects
/// that should work both in offline and online
public abstract class SmartNetBehaviour : NetworkBehaviour
{
	#region DATA
	public NetworkActor runsOn;
	#endregion

	#region CALLBACKS

	#region TESTING STILL
	private void Update () 
	{

	}
	protected virtual void OnUpdate () { }

	public void Call (Action call, NetworkActor onActor)
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

	}
	#endregion
}
