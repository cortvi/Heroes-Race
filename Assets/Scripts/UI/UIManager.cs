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
	[SyncVar]
	public Pantallas currentScreen;
	public Sprite[] personajes;
}

public enum Pantallas 
{
	/// Todas las pantallas en terminos
	/// de UI
	MenuPrincipal,
	SeleccionPersonaje,
	Loading,
	InGame
}
