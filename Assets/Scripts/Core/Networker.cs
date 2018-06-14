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
																Debug.LogError ("what is this", selectors[id]);
			selectors[id].id.AssignClientAuthority (conn);
																Debug.LogError ("what is this 2.0", selectors[id]);
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
