using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// Contiene referencias a diferentes objetos
/// de forma centralizada, así como funciones muy básicas
/// del funcionamiento del juego.
public class Game : NetworkBehaviour
{
	#region ID CONEXION
	/// Identificacion de cada recreativa
	/// por order de conexion con el servidor
	public static int id;

	[ClientRpc]
	public void Rpc_SavePlayerID( int id ) 
	{
		/// Recibe desde el ID de conexion
		/// desde el Servidor
		Game.id = id;
		print ("Connection ID: " + Game.id);
	}
	public static IEnumerator waitConnectionID ( int id ) 
	{
		/// Esperar hasta que se hayan conectado
		/// completamente los jugadores
		/// [Server-side]
		while (!manager) yield return null;

		manager.Rpc_SavePlayerID (id);
	}
	#endregion

	#region REFERENCIAS
	public static Game manager;					/// El propio script
	public static UIManager ui;					/// El script que controla el UI
	public static Networker net;				/// El manager de la red
	public static NetworkIdentity connection;   /// La conexion del cliente con el servidor
	#endregion

	#region COMMANDS
	[Command]
	public void Cmd_GiveControl( GameObject obj )
	{
		/// Cede la autoridad sobre el
		/// objeto a el cliente
		NetworkServer.SpawnWithClientAuthority (obj.gameObject, connectionToClient);
	}
	#endregion

	#region CALLBACKS
	private void Awake()
	{
		manager = this;
		ui = GetComponent<UIManager> ();
		net = NetworkManager.singleton as Networker;
		connection = GetComponent<NetworkIdentity> ();

		DontDestroyOnLoad (gameObject);
	} 
	#endregion
}
