using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

/// Rota el selector de personaje
/// y permite seleccionar uno
public class Selector : NetworkBehaviour
{
	public Image current;
	public Image next;
	public Sprite[] personajes;			// El orden tiene que coincidir con la enum!
	public GameObject focus;            // Marca cual es nuestro personje

	int charId;
	bool sliding;
	NetworkAnimator ui;

	private void Update() 
	{
		if (!hasAuthority) return;
		
		/// En caso de que se pulse tecla de mover
		/// ( y si no se está moviendo ya ),
		/// deslizar targeta de personaje.
		var dir = InputX.GetMovement ();
		if (!sliding && dir != 0)
		{
			sliding = true;												// Evitar cambio de personaje hasta terminar animacion
			CorrectSlideID ( (int)dir );                                // Seleccionar ID del siguiente personaje
			next.sprite = personajes[charId];							// Mostrar siguiente personaje
			ui.SetTrigger ((dir == -1) ? "SlideLeft" : "SlideRight");	// UI Trigger en base a la direccion del movimiento
			StartCoroutine ( Slide () );
		}
	}

	IEnumerator Slide ()
	{
		// Esperar duracion de la animacion
		yield return new WaitForSeconds (.5f);
		// Corregir imagen en base a la animacion!
		current.sprite = personajes[charId];
	}
	void CorrectSlideID ( int dir ) 
	{
		charId += dir;
		if (charId == -1) charId = personajes.Length-1;
		else
		if (charId == personajes.Length) charId = 0;
	}

	private void Awake() 
	{
		charId = Game.id;
		current.sprite = personajes[charId];
		ui = GetComponent<NetworkAnimator> ();
	}
}

public enum PJs 
{
	/// Lista de todos los personajes
	/// que se pueden seleccionar
	Gatete,
	Sir,
	Random_0,
	Random_1
	// En un futuro cuidado con
	// cambiar los nombres!
	// El orden tiene que coincidir con la array!
}
