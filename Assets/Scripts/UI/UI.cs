using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// Contiene las funciones que gestionan
/// y regulan el UI entre el cliente y el servidor
public class UI : NetworkBehaviour
{
	public static UI manager;

	[SyncVar]
	public Pantallas currentScreen;					// La pantalla (estado) actual del juego
	public enum Pantallas 
	{
		/// Todas las pantallas en terminos
		/// de UI
		MenuPrincipal,
		SeleccionPersonaje,
		TodosListos,
		Loading,
		ReadySteadyGo,
		InGame
	}

	public Sprite[] personajes;						// Los splasharts de los personajes jugables

	private void Awake() 
	{
		manager = this;
	}
}
