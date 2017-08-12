using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// Gestiona funcionalidades de red tanto
/// del cliente como del servidor.
public class Networker : NetworkManager
{
	#region SERVIDOR
	/// Las conexiones con las recreativas
	public static List<NetworkConnection> conns;
	public static Dictionary<NetworkConnection, GameObject> players;

	public override void OnServerAddPlayer( NetworkConnection conn, short playerControllerId ) 
	{
		var player = Instantiate (playerPrefab) as GameObject;
		NetworkServer.AddPlayerForConnection (conn, player, playerControllerId);
		/// Registrar la conexion de cada
		/// player cuando se conecta
		conns.Add (conn);
		players.Add (conn, player);
	}
	#endregion

	#region CLIENTE
	// TODO
	#endregion

	#region CALLBACKS
	private void Awake() 
	{
		conns = new List<NetworkConnection> (3);
		players = new Dictionary<NetworkConnection, GameObject> (3);
	}
	#endregion
}
