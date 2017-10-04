using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Networker : NetworkManager
{
	#region SERVIDOR
	public static int playerCount;
	public override void OnServerAddPlayer( NetworkConnection conn, short playerControllerId ) 
	{
		/// Crea un nuevo objeto con el script Game.cs 
		/// para la recreativa que se acaba de conectar
		var player = Instantiate (playerPrefab) as GameObject;

		/// Assign selector
		var selectors = UI.manager.selectors;
		var id = playerCount++;
		selectors[id].GetComponent<NetworkIdentity> ().AssignClientAuthority (conn);
		selectors[id].pj = ( PJs ) id;
		selectors[id].current.sprite = UI.manager.personajes[id];
		selectors[id].owner = player.GetComponent<Game> ();

		/// Add player
		NetworkServer.AddPlayerForConnection (conn, player, playerControllerId);
	}
	#endregion
}
