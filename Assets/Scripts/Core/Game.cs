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
	#region COMMANDS
	[Command]
	void Cmd_TriggerUI ( int trigger ) 
	{
		/// Cambia de pantalla para
		/// todas las recreativas
		var next = ( Pantallas ) trigger;
		UI.manager.currentScreen = next;

		#region SWITCH
		switch (next)
		{
			case Pantallas.SeleccionPersonaje:
				UI.manager.GetComponent<Animator> ().SetTrigger (next.ToString ());
				/// Otorga autoridad sobre los selectores
				var selectors = UI.manager.GetComponentsInChildren<Selector> (true);
				for (var s=0; s!=Networker.conns.Count; s++)
				{
					var nId = selectors[s].GetComponent<NetworkIdentity> ();
					nId.AssignClientAuthority (Networker.conns[s]);
				}
				break;
		}
		#endregion
	}
	#endregion

	#region CALLBACKS
	private void Update() 
	{
		#region CLIENTE
		/// Funcionalidades de los clientes
		if (isLocalPlayer)
		{
			/// Al pulsar el boton verde de la recreativa:
			if (InputX.GetKeyDown (PlayerActions.GreenBtn))
			{
				#region SWITCH
				/// El boton verde ejecuta acciones diferentes
				/// segun en que momento del juego nos encontremos.
				switch (UI.manager.currentScreen)
				{
					case Pantallas.MenuPrincipal:
						Cmd_TriggerUI ( (int) Pantallas.SeleccionPersonaje );
						break;
				}
				#endregion
			}
		}
		#endregion

		#region SERVIDOR
		/// Funcionalidades del servidor
		if (isServer)
		{
			if (InputX.GetKeyDown (DevActions.NetworkHUD))
			{
				/// Muestra/Oculta HUD del NetworkManager
				NetworkManager.singleton.GetComponent<NetworkManagerHUD> ().showGUI ^= true;   // invertir valor
			}
		}
		#endregion
	}

	private void Awake () 
	{
		DontDestroyOnLoad (gameObject);

		if (isClient)
		{
			// This way, any wrong call will generate
			// a NullReferenceException
			Networker.conns = null;
			Networker.players = null;
		}
	}
	#endregion
}
