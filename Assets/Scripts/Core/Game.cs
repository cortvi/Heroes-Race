using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// Contiene referencias a diferentes objetos
/// de forma centralizada, así como funciones muy básicas
/// del funcionamiento del juego.
public class Game : NetworkBehaviour
{
	#region CONNECTION ID
	[SyncVar] public int id;
	#endregion

	#region COMMANDS
	[Command]
	void Cmd_TriggerUI ( int trigger )
	{
		/// Cambia de pantalla para
		/// todas las recreativas
		var next = ( Pantallas ) trigger;
		ui.currentScreen = next;

		#region SWITCH
		switch (next)
		{
			case Pantallas.SeleccionPersonaje:
				ui.GetComponent<Animator> ().SetTrigger (next.ToString ());
				/// Otorga autoridad sobre los selectores
				var selectors = ui.GetComponentsInChildren<Selector> (true);
				for (var s=0; s!=net.players.Count; s++)
				{
					var nId = selectors[s].GetComponent<NetworkIdentity> ();
					nId.AssignClientAuthority (net.players[s]);
				}
				break;
		}
		#endregion
	}
	#endregion

	#region REFERENCIA
	public static UIManager ui;			/// El script que controla el UI
	public static Networker net;		/// El manager de la red
	#endregion

	#region CALLBACKS
	private void Update() 
	{
		#region CLIENTE
		/// Funcionalidades de los clientes
		if (isLocalPlayer)
		{
			/// Al pulsar el boton verde de la recreativa:
			if (InputX.GetKeyDown (PlayerActions.GreenBtn))
			{
				#region SWITCH
				/// El boton verde ejecuta acciones diferentes
				/// segun en que momento del juego nos encontremos.
				switch (ui.currentScreen)
				{
					case Pantallas.MenuPrincipal:
						Cmd_TriggerUI ( (int) Pantallas.SeleccionPersonaje );
						break;
				}
				#endregion
			}
		}
		#endregion

		#region SERVIDOR
		/// Funcionalidades del servidor
		if (isServer)
		{
			if (InputX.GetKeyDown (DevActions.NetworkHUD))
			{
				/// Muestra/Oculta HUD del NetworkManager
				net.GetComponent<NetworkManagerHUD> ().showGUI ^= true;   // invertir valor
			}
		}
		#endregion
	}

	private void Awake() 
	{
		ui = GameObject.Find ("Canvas").GetComponent<UIManager> ();
		net = NetworkManager.singleton as Networker;
		DontDestroyOnLoad (gameObject);
	}
	#endregion
}
