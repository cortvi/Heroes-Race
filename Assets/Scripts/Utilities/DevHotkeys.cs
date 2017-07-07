using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Utilities
{
	/// <summary>
	/// Atajos de teclado para funciones
	/// de control / debug / etc...
	/// </summary>
	public class DevHotkeys : MonoBehaviour
	{
		private void Update()
		{
			if ( Input.GetKeyDown ( (KeyCode) DevActions.NetworkHUD ) )
			{
				/// Muestra/Oculta HUD del NetworkManager
				GetComponent<NetworkManagerHUD> ().showGUI ^= true;		// invertir valor
			}
		} 
	}
}
