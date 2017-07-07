using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
