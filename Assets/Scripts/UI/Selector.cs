using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

/// Rota el selector de personaje
/// y permite seleccionar uno
public class Selector : NetworkBehaviour
{
	#region REFERENCES
	[SyncVar]
	public PJs pj;					// El personaje selccionado
	int charId 
	{
		get { return ( int ) pj; }
		set { pj = ( PJs ) value; }
	}

	public Image current;
	public Image next;
	public Sprite[] personajes;			// El orden tiene que coincidir con la enum!
	public GameObject focus;            // Marca cual es nuestro personje

	bool sliding;
	NetworkAnimator anim;
	#endregion

	#region SLIDING
	public void CorrectSprite () 
	{
		current.sprite = personajes[charId];
		sliding = false;
	}
	void CorrectSlideID ( int dir ) 
	{
		charId += dir;
		var max = personajes.Length;

		if (charId == -1) charId = max-1;
		else
		if (charId == max) charId = 0;
	}
	#endregion

	#region CALLBACKS
	private void Update() 
	{
		if (!hasAuthority || isServer) return;

		/// En caso de que se pulse tecla de mover
		/// ( y si no se está moviendo ya ),
		/// deslizar targeta de personaje.
		var dir = InputX.GetMovement ();
		if (!sliding && dir != 0)
		{
			sliding = true;													// Evitar cambio de personaje hasta terminar animacion
			CorrectSlideID (( int ) dir);									// Seleccionar ID del siguiente personaje
			next.sprite = personajes[charId];								// Mostrar siguiente personaje
			anim.SetTrigger ((dir == -1) ? "SlideLeft" : "SlideRight");		// UI Trigger en base a la direccion del movimiento
		}
	}

	public override void OnStartAuthority () 
	{
		base.OnStartAuthority ();
		current.sprite = personajes[charId];
		anim = GetComponent<NetworkAnimator> ();
		if (isClient) focus.SetActive (true);
	} 
	#endregion
}

public enum PJs 
{
	/// Lista de todos los personajes
	/// que se pueden seleccionar
	NONE,   // => Espectador
	Gatete,
	Sir,
	Random_0,
	Random_1
	// En un futuro cuidado con
	// cambiar los nombres! => El orden tiene que coincidir con la array!
}
