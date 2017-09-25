using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Networker : NetworkManager
{
	#region SERVIDOR
	/// Las conexiones con las recreativas
	public static List<NetworkConnection> conns;

	public override void OnServerAddPlayer( NetworkConnection conn, short playerControllerId ) 
	{
		/// Crea un nuevo objeto con el script Game.cs 
		/// para la recreativa que se acaba de conectar
		var player = Instantiate (playerPrefab) as GameObject;
		NetworkServer.AddPlayerForConnection (conn, player, playerControllerId);
		/// Registrar la conexion de cada
		/// player cuando se conecta
		player.GetComponent<Game> ().playerId = conns.Count;
		conns.Add (conn);
	}
	#endregion

	#region CLIENTE
	// TODO
	#endregion

	#region CALLBACKS
	private void Awake() 
	{
		/// Inicializacion
		conns = new List<NetworkConnection> (3);
	}
	#endregion
}
