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
	public Sprite[] personajes;			// El orden tiene que coincidir!
	public GameObject focus;			// Marca cual es nuestro personje

	private void Update() 
	{
		if (!hasAuthority) return;


	}
}

public enum PJs 
{
	/// Lista de todos los personajes
	/// que se pueden seleccionar
	Gatete,
	Random_1,
	Random_0,
	Sir
	// En un futuro cuidado con
	// cambiar los nombres!
}
