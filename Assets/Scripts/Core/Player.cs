using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// Este es el script principal de los personajes
///	que controla cada jugador.
public class Player : NetworkBehaviour
{
	private void Update()
	{
		/// Cada cliente conrtola SOLO su personaje
		if ( !isClient || !hasAuthority) return;

		// TO-DO
	}
}
