using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EsponjaAnimEvents : MonoBehaviour
{
	void ThrowPlayers () 
	{
		// Ejecuta la funcion real
		prop.ThrowPlayers ();
	}

	Esponja prop;
	private void Awake() 
	{
		prop = GetComponentInChildren<Esponja> ();
	}
}
