using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Game : NetworkBehaviour
{
	[SyncVar] public PJs pj;

	#region CALLBACKS
	private void Update() 
	{
		#region SERVIDOR
		/// Funcionalidades del servidor
		if (isServer)
		{
			if (InputX.GetKeyDown (DevActions.NetworkHUD))
			{
				/// Muestra/Oculta HUD del NetworkManager
				NetworkManager.singleton.GetComponent<NetworkManagerHUD> ().showGUI ^= true;   // invertir visibilidad
			}
		}
		#endregion
	}

	private void Awake () 
	{
		/// Evita que se destruya este objeto
		/// al cargar otros niveles
		DontDestroyOnLoad (gameObject);
//		Cursor.visible = false;
//		Cursor.lockState = CursorLockMode.Locked;
	}
	#endregion
}
