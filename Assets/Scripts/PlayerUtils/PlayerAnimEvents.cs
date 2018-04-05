using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimEvents : MonoBehaviour
{
	void Jump() 
	{
		if (player.OnAir) return;
		/// Esta funcion se llama desde la animacion de salto,
		/// aplica una fuerza hacia arriba en el Rigidbody
		var jumpDir = transform.up * player.jumpForce;
		player.body.AddForceAtPosition (jumpDir, transform.position, ForceMode.VelocityChange);
		player.OnAir = true;
	}

	Character player;
	private void Awake() 
	{
		player = transform.parent.GetComponent<Character> ();
	}
}
