using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Este es el script principal de los personajes
///	que controla cada jugador.
/// </summary>
public class Player : NetworkBehaviour
{
	private void Update()
	{
		/// Solo el cliente debe poder
		/// controlar SOLO su personaje.
		if (!isLocalPlayer)
			return;

		// TODO
	}
}
