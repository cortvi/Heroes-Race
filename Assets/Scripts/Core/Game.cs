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
	[SyncVar]
	public int id;

	#endregion

	#region COMMANDS
	[Command]
	public void Cmd_GiveControl( NetworkIdentity nid )
	{
		/// Cede la autoridad sobre el
		/// objeto a el cliente
		nid.AssignClientAuthority (connectionToClient);
		print ("Assigned authority over " + nid.gameObject.name + "to ->\t" + name);
	}
	#endregion

	#region REFERENCIA
	/// Las referencias solo son
	/// validas dentro de cada
	/// cliente!
	public static List<Game> players;		/// Player objects de cada jugador
	public UIManager ui;					/// El script que controla el UI
	public Networker net;					/// El manager de la red
	#endregion

	#region CALLBACKS
	public override void OnStartLocalPlayer() 
	{
		base.OnStartLocalPlayer ();
		manager = this;
		ui = GetComponent<UIManager> ();
		net = NetworkManager.singleton as Networker;
		connection = GetComponent<NetworkIdentity> ();
	}

	private void Awake() 
	{
		DontDestroyOnLoad (gameObject);
	}
	#endregion
}
