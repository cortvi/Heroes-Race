using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// Gestiona los cambios y cargas de niveles
public class Levels : NetworkBehaviour
{ 
	public static Levels manager;
	enum Names 
	{
		/// Las diferentes escenas/niveles
		/// De momento solo tenemos la torre de agua
		MainMenu,
		WaterTower
	}

	private void OnLevelWasLoaded( int level ) 
	{
		/// Logica a ejecutar cuando se carguen
		/// determinados niveles
		switch ((Names) level)
		{
		case Names.WaterTower:

			break;
		}
	}

	private void Awake() 
	{
		manager = this;
	}
}
