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
	public void Cmd_GiveControl( NetworkIdentity nid )
	{
		#region IGNORAR
		/// Si el objeto no estuviera aun por aparecer (animacion de UI, etc)
		/// se espera a que aparezca
		//		if ( NetworkServer.FindLocalObject (nid.netId) == null )
		//		{
		//			StartCoroutine (WaitControl (nid.netId));
		//			return;
		//		} 
		#endregion

		/// Cede la autoridad sobre el
		/// objeto a el cliente
		nid.AssignClientAuthority (connectionToClient);
		print ("Assigned authority over " + nid.gameObject.name);
	}

	#region IGNORAR
	///	IEnumerator WaitControl( NetworkInstanceId id )
	///	{
	///		float time=0;
	///		GameObject obj = NetworkServer.FindLocalObject (id);
	///		while (obj == null)
	///		{
	///			/// Espera a que el objeto este disponible para dar autoridad.
	///			/// Maximo 5 segundos.
	///			/// Si tarda mas algo no va bien por algun otro sitio, o habria que crear
	///			/// una corutina especializada?
	///			obj = NetworkServer.FindLocalObject (id);
	///			time += Time.deltaTime;
	///			if (time >= 5f)
	///				throw new Exception ("NO SE HA PODIDO ASIGNAR AUTORIDAD. ALGO VA MAL?");
	///			else
	///				yield return null;
	///		}
	///
	///		obj.GetComponent<NetworkIdentity> ().AssignClientAuthority (connectionToClient);
	///		print ("Assigned authority after " + time + "s");
	///	} 
	#endregion
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
