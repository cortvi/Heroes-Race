using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UI : NetworkBehaviour
{
	public static UI manager;						// Referencia estática a sí mismo

	[SyncVar]
	public Pantallas currentScreen;					// La pantalla (estado) actual del juego (sincronizado en red)
	public enum Pantallas 
	{
		/// Todas las pantallas
		/// en terminos de UI
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
