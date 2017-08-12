using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Mapea el input tanto de teclado como de
/// recreativa a los controles del juego.
public static class InputX
{
	/// Funciones alternativas a la clase Input de Unity
	public static bool GetKey ( PlayerActions action ) { return Input.GetKey (( KeyCode ) action); }
	public static bool GetKey ( DevActions action ) { return Input.GetKey (( KeyCode ) action); }

	public static bool GetKeyDown ( PlayerActions action ) { return Input.GetKeyDown (( KeyCode ) action); }
	public static bool GetKeyDown ( DevActions action ) { return Input.GetKeyDown (( KeyCode ) action); }

	public static bool GetKeyUp ( PlayerActions action ) { return Input.GetKeyUp (( KeyCode ) action); }
	public static bool GetKeyUp ( DevActions action ) { return Input.GetKeyUp (( KeyCode ) action); }

	public static float GetMovement () 
	{
		/// Devuelve:
		/// 1  si el personaje se esta moviendo hacia la derecha,
		/// -1 si se esta moviendo hacia la izquierda.s
		if (GetKey (PlayerActions.MoveLeft)) return -1;
		else
		if (GetKey (PlayerActions.MoveRight)) return 1;

		else return 0;
	}
}

#region KEYS
/// Todas las acciones que puede llevar a cabo el juaador.
/// Cada accion equivale a un KeyCode.
public enum PlayerActions 
{
	// De momento esta hard-coded, pero esto facilita
	// que en un futuro se puedan cambiar facilmente los controles.
	// Basta con cambiar el valolr aquí para que se cambie en el resto
	// del juego.
	MoveLeft = KeyCode.A,
	MoveRight = KeyCode.D,
	Jump = KeyCode.Space,
	Dash = KeyCode.Q,
	Attack = KeyCode.W,
	PowerUp = KeyCode.E,
	GreenBtn = KeyCode.Return
}

/// Las teclas ( KeyCode ) que corresponden
/// a los atajos de desarrollador.
/// Hard-coded.
public enum DevActions 
{
	NetworkHUD = KeyCode.Backslash
}
#endregion
