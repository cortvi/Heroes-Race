using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
	/// <summary>
	/// Mapea el input tanto de teclado como de
	/// recreativa a los controles del juego.
	/// </summary>
	public class InputX : MonoBehaviour
	{
		// Funciones alternativas
		public static bool GetKey ( PlayerActions action ) { return Input.GetKey (( KeyCode ) action); }
		public static bool GetKeyDown ( PlayerActions action ) { return Input.GetKeyDown (( KeyCode ) action); }
		public static bool GetKeyUp ( PlayerActions action ) { return Input.GetKeyUp (( KeyCode ) action); }

		/// <summary>
		/// Devuelve:
		/// 1  si el personaje se esta moviendo hacia la derecha,
		/// -1 si se esta moviendo hacia la izquierda.s
		/// </summary>
		public static float GetMovement ()
		{
			if (GetKey (PlayerActions.MoveLeft)) return -1;
			else
			if (GetKey (PlayerActions.MoveRight)) return 1;

			else return 0;
		}
	}

	#region KEYS
	/// <summary>
	/// Todas las acciones que puede llevar a cabo el juaador.
	/// Cada accion equivale a un KeyCode.
	/// </summary>
	public enum PlayerActions 
	{
		// De momento esta hard-coded, pero esto facilita
		// que en un futuro se puedan cambiar facilmente los controles.
		MoveLeft = KeyCode.LeftArrow,
		MoveRight = KeyCode.RightArrow,
		Jump = KeyCode.Space,
		Dash = KeyCode.Q,
		Attack = KeyCode.W,
		PowerUp = KeyCode.E
	}


	/// Las teclas ( KeyCode ) que corresponden
	/// a los atajos de desarrollador.
	/// Hard-coded.
	public enum DevActions 
	{
		NetworkHUD = KeyCode.Backslash
	}
	#endregion
}
