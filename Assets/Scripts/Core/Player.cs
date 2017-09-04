using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : MonoBehaviour
{
	#region INTERNAL DATA
	public float jumpForce;				// Fuerza del salto
	public float charSpeed;				// La velocidad del personaje
	public float runSpeedOffset;        // Añadido a la velocidad base de la animacion de correr

	Transform parent;					// El parent del objeto, se rota para dar la sensación de movimiento
	Rigidbody body;						// El 'Rigidbody' que se encarga de algunas físicas del personaje
	Animator anim;						// El 'Animator' que gestiona las animaciones del personaje
	#endregion

	#region ANIMATON PARAMS
	// Multiplicador de velocidad de la animacion de
	// movimiento del personaje
	float SpeedMul 
	{
		get { return anim.GetFloat ("SpeedMul"); } 
		set { anim.SetFloat ("SpeedMul", value); }
	}
	// Si es TRUE, se activa la animacion de movimiento,
	// si es FALSE, se detiene
	bool Moving 
	{
		get { return anim.GetBool ("Moving"); }
		set { anim.SetBool ("Moving", value); }
	}
	// Si el personaje está, o no, el aire tras un salto.
	bool OnAir
	{
		get { return anim.GetBool ("OnAir"); }
		set { anim.SetBool ("OnAir", value); }
	}
	#endregion

	#region MOVEMENT
	void Movement (float dir) 
	{
		if (dir != 0)
		{
			Moving = true;
			SpeedMul = Mathf.Abs (dir) + runSpeedOffset;
			/// Rotamos el parent del Transform para simular el movimiento circular
			parent.Rotate (Vector3.up, charSpeed * Time.deltaTime * -dir);
		}
		else Moving = false;
	}
	#endregion

	#region ROTATION
	/// La direccion del movimiento hasta ahora
	/// 0->Hacia la izquierda | 1->Hacia la derecha
	float currentDirection = 1;
	void Rotation( float dir )
	{
		/// Rota 180 al cambiar direccion de movimiento
		if (dir != 0 && dir != currentDirection)
		{
			// De momento la transicion será dura,
			// luego habra que suavizarla
			transform.Rotate (Vector3.up, 180);
			currentDirection = dir;
		}
	}
	/// aG9sYSBwaWUgOjM=
	#endregion

	#region JUMPING
	void JumpCheck () 
	{
		/// Saltamos al presionar la tecla,
		/// y si NO estamos ya en el aire
		if ( InputX.GetKeyDown ( PlayerActions.Jump ) && !OnAir )
		{
			/// Trigger animaciones de salto
			anim.SetTrigger ("Jump");
		}
	}
	void Jump () 
	{
		/// Esta funcion se llama desde la animacion de salto,
		/// aplica una fuerza hacia arriba en el Rigidbody
		var jumpDir = (transform.forward + transform.up) * jumpForce;
		body.AddForceAtPosition (jumpDir, transform.position, ForceMode.VelocityChange);
		/// Ahora estamos en el aire
		OnAir = true;
	}
	#endregion

	#region CALLBACKS
	private void Update()
	{
		/// Cada cliente conrtola SOLO su personaje
//		if ( !isClient || !hasAuthority) return;
// Esta linea esta comentada
// para trabajar con el
// personaje sin red!

		var dir = InputX.GetMovement ();
		Movement (dir);
		Rotation (dir);
		JumpCheck ();
	}
	private void Awake() 
	{
		/// Referencias internas
		parent = transform.parent;
		anim = GetComponent<Animator> ();
		body = GetComponent<Rigidbody> ();
	} 

	private void OnCollisionEnter( Collision col ) 
	{
		/// Checks de colision
		switch (col.gameObject.tag)
		{
			case "Floor":
			{
				if (OnAir)
				{
					/// Al chocar con el suelo, estando en el aire,
					/// cerramos la animacion de salto y dejamas de estar
					/// en el aire.
					OnAir = false;
					anim.SetTrigger ( "Land" );
				}
			}
			break;
		}
	}
	#endregion
}
