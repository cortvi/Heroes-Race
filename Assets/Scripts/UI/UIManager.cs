using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// Contiene las funciones que gestionan
/// y regulan el UI entre el cliente y el servidor.
/// </summary>
public class UIManager : NetworkBehaviour
{
	#region GESTION DEL UI
	/// <summary>
	/// Todas las pantallas en terminos
	/// de UI.
	/// </summary>
	private enum Pantallas 
	{
		MenuPrincipal,
		SeleccionPersonaje,
		Loading,
		InGame
	}
	private Pantallas currentScreen;
	#endregion

	private void Update()
	{
		/// Funcionalidades de los clientes
		if (isLocalPlayer)
		{
			// Al pulsar el boton verde de la recreativa:
			if ( InputX.GetKeyDown ( PlayerActions.GreenBtn ) )
			{
				/// El boton verde ejecuta acciones diferentes
				/// segun en que momento del juego nos encontremos.
				switch ( currentScreen )
				{
					case Pantallas.MenuPrincipal:
						// Ir a seleccion de personaje
						// en todas las recreativas a la vez
						ui.GetComponent<NetworkAnimator> ().SetTrigger ("SeleccionPersonaje");
						break;
				}
			}
		}

		/// Funcionalidades del servidor
		if (isServer)
		{
			if ( InputX.GetKeyDown (DevActions.NetworkHUD) )
			{
				/// Muestra/Oculta HUD del NetworkManager
				NetworkManager.singleton.GetComponent<NetworkManagerHUD> ().showGUI ^= true;   // invertir valor
			}
		}
	}

	#region COMMANDS
	/// Estas funcionas se llaman desde los clientes
	/// y se ejecutan en el Servidor.
	[Command]
	void TriggerTest () { ui.SetTrigger ("SeleccionPersonaje"); }
	#endregion

	Animator ui;
	private void Awake()
	{
		ui = GameObject.Find ("Canvas").GetComponent<Animator> ();
	}
}
