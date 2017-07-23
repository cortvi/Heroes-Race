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
	// Las conexiones con las recreativas
	static int playerCount;

	public override void OnServerAddPlayer( NetworkConnection conn, short playerControllerId )
	{
		base.OnServerAddPlayer (conn, playerControllerId);
		// Registrar la conexion de cada
		// player cuando se conecta
		FindObjectsOfType<Game> ().Last ().Target_SavePlayerID (conn, playerCount);
		playerCount++;
	}
	#endregion

	#region CLIENTE
	// TODO
	#endregion
}
