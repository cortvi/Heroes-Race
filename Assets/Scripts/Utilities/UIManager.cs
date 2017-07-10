using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Contiene las funciones que se llaman
/// desde el UI.
/// </summary>
public class UIManager : MonoBehaviour
{
	#region REFERENCIAS
	// TODO
	#endregion

	public void NuevaPartida ()
	{
		// Solo empezar partida si se tiene conexion
		if (NetworkManager.singleton.isNetworkActive)
		{
			// TODO:
			// Iniciar seleccion de campeon
			// en TODAS las recreativas
		}
	}

	public void Creditos ()
	{
		// TODO:
		// Mostrar la pantalla de creditos
		// ( NO en todas las recreativas )
	}

	// Salir del juego
	public void Salir () { Application.Quit (); } 
}
