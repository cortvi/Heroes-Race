using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Contiene referencias a diferentes objetos
/// de forma centralizada, así como funciones muy básicas
/// del funcionamiento del juego.
/// </summary>
public class Game : NetworkBehaviour
{
	#region ID CONEXION
	/// ID de conexion [0-2]
	public static int ID;

	[ClientRpc]
	public void Rpc_SavePlayerID( int id )
	{
		/// Recibe desde el ID de conexion
		/// desde el Servidor
		ID = id;
	}
	#endregion

	#region REFERENCIAS
	public static Game manager;         /// El propio script
	public static UIManager ui;         /// El script que controla el UI
	public static Networker net;		/// El manager de la red
	#endregion

	private void Awake() 
	{
		manager = this;
		ui = GetComponent<UIManager> ();
		net = NetworkManager.singleton as Networker;
		DontDestroyOnLoad (gameObject);
	}
}
