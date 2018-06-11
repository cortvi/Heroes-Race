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

		// Behaviour on what scene where at
		string scene = SceneManager.GetActiveScene ().name;
		if (scene == "Main") 
		{
			// Assign authority to selectors
			int id = conn.connectionId - 1;
			var selectors = FindObjectsOfType<Selector> ();
			selectors[id].id.AssignClientAuthority (conn);
			selectors[id].SetName ("Selector");

		}
		else
		if (scene == "Testing")
		{
			// spawn characters for testing the quesitos
		}
	}
	#endregion

	#region CALLBACKS
	[RuntimeInitializeOnLoadMethod
	(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void InitizalizeSingleton () 
	{
		i = Extensions.SpawnSingleton <Networker> ();
	}
	#endregion
}
