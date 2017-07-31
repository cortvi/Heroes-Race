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
	public static List<NetworkConnection> players = new List<NetworkConnection> (3);

	public override void OnServerAddPlayer( NetworkConnection conn, short playerControllerId )
	{
		/// Registrar la conexion de cada
		/// player cuando se conecta
		var player = Instantiate (playerPrefab) as GameObject;
		NetworkServer.AddPlayerForConnection (conn, player, playerControllerId);

		players.Add (conn);
	}
	#endregion

	#region CLIENTE
	// TODO
	#endregion
}
