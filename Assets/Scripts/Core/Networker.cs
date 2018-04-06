using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class Networker : NetworkManager 
{
	#region DATA
	public static Networker main;				/// Singleton

	/// Las conexiones con las recreativas
	public static List<NetworkConnection> conns;
	public static Dictionary<NetworkConnection, Game> players;
	#endregion

	#region SERVER
	public override void OnServerAddPlayer (NetworkConnection conn, short playerControllerId) 
	{
		/// Crea un nuevo objeto con el script Game.cs 
		/// para la recreativa que se acaba de conectar
		var player = Instantiate (playerPrefab) as GameObject;
		NetworkServer.AddPlayerForConnection (conn, player, playerControllerId);

		/// Registrar la conexion de cada
		/// player cuando se conecta
		conns.Add (conn);
		players.Add (conn, player.GetComponent<Game> ());
	}
	#endregion

	#region CALLBACKS
	private void Awake() 
	{
		/// Inicializacion
		conns = new List<NetworkConnection> (3);
		players = new Dictionary<NetworkConnection, Game> (3);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void InitizalizeSingleton () 
	{
		main = Extensions.SpawnSingleton <Networker> ();
	}
	#endregion
}
