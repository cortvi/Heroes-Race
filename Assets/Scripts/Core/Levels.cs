using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// Gestiona los cambios y cargas
/// de niveles
public class Levels : NetworkBehaviour
{ 
	enum Names 
	{
		MainMenu,
		Torre
	}
	public static Levels manager;

	private void OnLevelWasLoaded( int level ) 
	{
		switch ((Names) level)
		{
			case Names.Torre:

				break;
		}
	}

	private void Awake() 
	{
		manager = this;
	}
}
