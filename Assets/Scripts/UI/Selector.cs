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

	private void Update() 
	{
		if (!hasAuthority) return;
		
		/// En caso de que se pulse tecla de mover
		/// ( y si no se está moviendo ya ),
		/// deslizar targeta de personaje.
		var dir = InputX.GetMovement ();
		if (!sliding && dir != 0) StartCoroutine (Slide (dir));
	}
	private void Awake() 
	{
		charId = Game.id;
		current.sprite = personajes[charId];
	}

	IEnumerator Slide ( float dir )
	{
		// Posicionar siguiente slide
		next.rectTransform.position = current.rectTransform.position + Vector3.right * 100f * dir;
		// Cambiar imagen en siguiente slide
		CorrectSlideID (( int ) dir);
		next.sprite = personajes[charId];

		var progress = 0f;
		while ( progress <= 1 )
		{
			yield return null;
		}
	}

	void CorrectSlideID ( int dir )
	{
		charId += dir;
		if (charId == -1) charId = personajes.Length-1;
		else
		if (charId == personajes.Length) charId = 0;
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
