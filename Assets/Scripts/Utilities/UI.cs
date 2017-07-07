using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Utilities
{
	/// <summary>
	/// Contiene las funciones que se llaman
	/// desde el UI
	/// </summary>
	public class UI : MonoBehaviour
	{
		public void NuevaPartida()
		{
			// TODO:
			// Iniciar seleccion de campeon en TODAS
			// las recreativas.

			// Por ahora salto directamente al juego
			GetComponent<NetworkManager> ().ServerChangeScene ("Torre");
		}

		public void Creditos()
		{
			// TODO:
			// Mostrar la pantalla de creditos
		}

		// Salir del juego
		public void Salir() { Application.Quit (); } 
	}
}
