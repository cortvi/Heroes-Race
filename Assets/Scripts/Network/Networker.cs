using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class Networker : NetworkManager 
{
	#region DATA
	public static Networker i;
	public static List<Game> players;

	public static bool DedicatedServer 
	{
		get { return (NetworkServer.active && !NetworkServer.localClientActive); }
	}
	public static bool DedicatedClient 
	{
		get { return (NetworkClient.active && !NetworkServer.localClientActive); }
	}
	public static bool IsHost 
	{
		get { return NetworkServer.localClientActive; }
	}
	#endregion

	#region SERVER
	public override void OnServerAddPlayer (NetworkConnection conn, short playerControllerId) 
	{
		// Spawn player object over the net
		var player = Instantiate (playerPrefab).GetComponent<Game> ();
		NetworkServer.AddPlayerForConnection (conn, player.gameObject, playerControllerId);
		player.SetName ("Player");
		players.Add (player);
	}
	#endregion

	#region CALLBACKS
	[RuntimeInitializeOnLoadMethod
	(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void InitizalizeSingleton () 
	{
		i = Extensions.SpawnSingleton <Networker> ();
		players = new List<Game> (3);
	}
	#endregion
}
