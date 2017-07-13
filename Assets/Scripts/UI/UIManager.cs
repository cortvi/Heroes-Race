using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// Contiene las funciones que se llaman
/// desde el UI.
/// </summary>
public class UIManager : NetworkBehaviour
{
	#region REFERENCIAS
	[Header ("Seleccion de personaje")]
	public GameObject seleccionPersonaje;
	#endregion

	#region UI MANAGEMENT
	/// Controla en que pantalla de los menus estan
	/// todas las recreativas.
	[SyncVar (hook = "OnChangedScreen")]
	private string screen;

	void OnChangedScreen ( string newScene )
	{
		// TODO:
		// Comprovar nombre y cambiar
		// UI a esa pantalla
	}
	#endregion

	[Command]
	private void Cmd_NuevaPartida ()
	{
		print ("Hola");
	}

	public void NuevaPartida ()
	{
		// Solo empezar partida si se tiene conexion
		if (NetworkManager.singleton.isNetworkActive)
		{
			// TODO:
			// Iniciar seleccion de campeon
			// en TODAS las recreativas
			Cmd_NuevaPartida ();
		}
	}

	public void Creditos ()
	{
		// TODO:
		// Mostrar la pantalla de creditos
		// ( NO en todas las recreativas )

		// Alomejor no hace falta funcion,
		// se puede hacer desde los eventos del inspector!
	}

	// Salir del juego
	public void Salir () { Application.Quit (); } 
}
