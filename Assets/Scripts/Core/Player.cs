using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// Este es el script principal de los personajes
///	que controla cada jugador.
public class Player : MonoBehaviour
{
	#region INTERNAL DATA
	public float jumpForce;				// Fuerza de salto
	public float charSpeed;				// La velocidad (metros) del personaje
	public float runSpeedOffset;		// Añadido a la velocidad base de la animacion

	Rigidbody body;
	Animator anim;
	#endregion

	#region ANIMATON PARAMS
	float SpeedMul 
	{
		set { anim.SetFloat ("SpeedMul", value); }
	}
	bool Moving 
	{
		set { anim.SetBool ("Moving", value); }
	}
	#endregion

	#region MOVEMENT
	void Movement (float mov) 
	{
		if (onAir)
		{
			body.AddForce (transform.forward * 100 * Time.deltaTime, ForceMode.VelocityChange);
			return;
		}

		Moving = (mov!=0 ? true : false);
		if (mov != 0)
		{
			SpeedMul = Mathf.Abs (mov) + runSpeedOffset;
			transform.Translate (0, 0, mov * direction * charSpeed * Time.deltaTime);
		}
	}
	#endregion

	#region ROTATION
	float direction = 1;
	void Rotation( float dir )
	{
		if (dir != direction && dir!=0)
		{
			// De momento la transicion sera dura
			// Luego habra que suavizarla teniendo en cuenta
			// el movimiento circular
			transform.Rotate (Vector3.up, 180);
			direction = dir;
		}
	}
	#endregion

	#region JUMPING
	bool onAir;
	void AirControl () 
	{
		if ( InputX.GetKeyDown ( PlayerActions.Jump ) && !onAir )
		{
			anim.SetTrigger ("Jump");
		}
	}
	void Jump () 
	{
		var jumpDir = (transform.forward + transform.up) * jumpForce;
		body.AddForceAtPosition (jumpDir, transform.position, ForceMode.VelocityChange);
		onAir = true;
	}
	#endregion

	#region CALLBACKS
	private void Update()
	{
		/// Cada cliente conrtola SOLO su personaje
//		if ( !isClient || !hasAuthority) return;

		var mov = InputX.GetMovement ();
		Rotation (mov);
		Movement (mov);
		AirControl ();
	}
	private void Awake() 
	{
		anim = GetComponent<Animator> ();
		body = GetComponent<Rigidbody> ();
	} 

	private void OnCollisionEnter( Collision col ) 
	{
		switch (col.gameObject.tag)
		{
		case "Floor":
			if (onAir)
			{
				onAir = false;
				anim.SetTrigger ( "Land" );
			}
		break;
		}
	}
	#endregion
}
