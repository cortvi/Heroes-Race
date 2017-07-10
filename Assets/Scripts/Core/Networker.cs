using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Gestiona funcionalidades de red tanto
/// del cliente como del servidor.
/// </summary>
public class Networker : NetworkManager
{
	#region SERVIDOR
	// Las conexiones con las recreativas
	NetworkConnection[] arcades = new NetworkConnection[3];
	int playerCount;

	public override void OnServerConnect( NetworkConnection conn )
	{
		base.OnServerConnect (conn);
		// Registrar la conexion de cada
		// player cuando se conecta
		arcades[playerCount] = conn;
		playerCount++;
	}
	#endregion

	#region CLIENTE
	// TODO
	#endregion
} 
