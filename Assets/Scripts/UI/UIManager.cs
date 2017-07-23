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
	#region REFERENCIAS
	Animator ui;            // El animator de todo el UI
	Selector[] selectors;   // Los selectores de personaje 
	#endregion

	#region GESTION DEL UI
	/// Todas las pantallas en terminos
	/// de UI.
	[SyncVar (hook = "OnScreenChange")]
	private Pantallas currentScreen;
	private enum Pantallas 
	{
		MenuPrincipal,
		SeleccionPersonaje,
		Loading,
		InGame
	}
	/// Logica ejecutada en los clientes
	/// al cambiar de pantalla
	void OnScreenChange ( Pantallas next ) 
	{
		switch ( next )
		{
			case Pantallas.SeleccionPersonaje:
				/// Al entrar en la pantalla de seleccion
				/// de personaje, los clientes deben asignar
				/// autoridad sobre su selector
				var selector = selectors[Game.manager.id];
				Game.manager.Cmd_GiveControl (selector.GetComponent<NetworkIdentity> ());
				// Activar la marca de focus!
				selector.focus.SetActive (true);
				break;
		}

		currentScreen = next;
	}
	#endregion

	#region COMMANDS
	[Command]
	void Cmd_TriggerUI ( string trigger ) 
	{
		/// Transicion entre pantallas
		/// del UI en Red
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
				#region SWITCH
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
				#endregion
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
		selectors = ui.GetComponentsInChildren<Selector> (true);
	} 
	#endregion
}
