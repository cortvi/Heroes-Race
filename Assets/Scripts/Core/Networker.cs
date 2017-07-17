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

	public override void OnServerConnect( NetworkConnection conn ) 
	{
		base.OnServerConnect (conn);
		// Registrar la conexion de cada
		// player cuando se conecta
		StartCoroutine (Game.waitConnectionID (playerCount));
		playerCount++;
	}
	#endregion

	#region CLIENTE
	// TODO
	#endregion
}
