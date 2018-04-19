using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class Networker : NetworkManager 
{
	#region DATA
	public static Networker main;					/// Singleton


	/// True if dedicacted Server
	public static bool DedicatedServer 
	{
		get { return (NetworkServer.active && !NetworkServer.localClientActive); }
	}

	/// True if dedicated Client
	public static bool DedicatedClient 
	{
		get { return (NetworkClient.active && !NetworkServer.localClientActive); }
	}

	/// True if acting as both client and server
	public static bool IsHost 
	{
		get { return NetworkServer.localClientActive; }
	}
	#endregion

	#region SERVER
	public override void OnServerAddPlayer (NetworkConnection conn, short playerControllerId) 
	{
		var player = Instantiate (playerPrefab).GetComponent<Game> ();
		player.netName = "Player";

		NetworkServer.AddPlayerForConnection (conn, player.gameObject, playerControllerId);
	}
	#endregion

	#region CALLBACKS
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void InitizalizeSingleton () 
	{
		main = Extensions.SpawnSingleton <Networker> ();
	}
	#endregion
}
