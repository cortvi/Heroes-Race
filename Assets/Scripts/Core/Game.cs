using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Contiene referencias a diferentes objetos
/// de forma centralizada, así como funciones muy básicas
/// del funcionamiento del juego.
/// </summary>
public class Game : MonoBehaviour
{
	/// Referencia statica a si mismo.
	public static Game manager;

	private void Awake()
	{
		// Inicializar referencias
		manager = this;
		DontDestroyOnLoad (gameObject);
	}
}
