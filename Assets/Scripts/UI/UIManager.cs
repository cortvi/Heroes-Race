using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// Contiene las funciones que gestionan
/// y regulan el UI entre el cliente y el servidor.
public class UIManager : NetworkBehaviour
{
	#region GESTION DEL UI
	/// Todas las pantallas en terminos
	/// de UI.
	[SyncVar]
	private Pantallas currentScreen;
	private enum Pantallas 
	{
		MenuPrincipal,
		SeleccionPersonaje,
		Loading,
		InGame
	}

	/// Referencias
	Animator ui;
	#endregion

	#region COMMANDS
	/// Transicion entre pantallas
	/// del UI en Red
	[Command]
	void Cmd_TriggerUI ( string trigger )
	{
		ui.SetTrigger (trigger);
		// Actualizar variable de control
		currentScreen = (Pantallas) Enum.Parse (typeof (Pantallas), trigger, true);
	}
	#endregion

	#region CALLBACKS
	private void Update()
	{
		/// Funcionalidades de los clientes
		#region CLIENTE
		if (isLocalPlayer)
		{
			/// Al pulsar el boton verde de la recreativa:
			if (InputX.GetKeyDown (PlayerActions.GreenBtn))
			{
				/// El boton verde ejecuta acciones diferentes
				/// segun en que momento del juego nos encontremos.
				switch (currentScreen)
				{
					case Pantallas.MenuPrincipal:
						// Ir a seleccion de personaje
						// en todas las recreativas a la vez
						Cmd_TriggerUI ("SeleccionPersonaje");
						
						break;
				}
			}
		}
		#endregion

		/// Funcionalidades del servidor
		#region SERVIDOR
		if (isServer)
		{
			if (InputX.GetKeyDown (DevActions.NetworkHUD))
			{
				/// Muestra/Oculta HUD del NetworkManager
				Game.net.GetComponent<NetworkManagerHUD> ().showGUI ^= true;   // invertir valor
			}
		} 
		#endregion
	}

	private void Awake() 
	{
		ui = GameObject.Find ("Canvas").GetComponent<Animator> ();
	} 
	#endregion
}
