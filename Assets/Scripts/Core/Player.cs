using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : MonoBehaviour
{
	#region INTERNAL DATA
	[Header ("References")]
	public Animator anim;				// El Animator del personaje
	public Animator cam;                // El Animator de la camara

	[Header ("PU Refs")]
	public GameObject shield;
//	public GameObject bomb;
	
	[Header ("Params")]
	public float jumpForce;				// Fuerza del salto
	public float charSpeed;				// La velocidad del personaje
	public float runSpeedMul;			// Añadido a la velocidad base de la animacion de correr

	[HideInInspector] public PU powerUp;
	[HideInInspector] public bool shielded;

	[HideInInspector] public Rigidbody body;	// El 'Rigidbody' que se encarga de algunas físicas del personaje
	[HideInInspector] public bool cannotWork;	// Esta bloqueada la accion del jugador?
	[HideInInspector] public bool cannotJump;   // Esta bloqueado el salto?
	CapsuleCollider playerCapsule;
	#endregion

	#region ANIMATON PARAMS
	// Multiplicador de velocidad de la animacion de
	// movimiento del personaje
	public float SpeedMul 
	{
		get { return anim.GetFloat ("SpeedMul"); } 
		set { anim.SetFloat ("SpeedMul", value); }
	}
	// Si es TRUE, se activa la animacion de movimiento,
	// si es FALSE, se detiene
	public bool Moving 
	{
		get { return anim.GetBool ("Moving"); }
		set { anim.SetBool ("Moving", value); }
	}
	// Si el personaje está, o no,
	// el aire tras un salto.
	public bool OnAir
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
			/// Rotamos rigidbody para simular el movimiento circular
			var q = Quaternion.Euler (0f, charSpeed * Time.fixedDeltaTime * -dir, 0f);
			body.MoveRotation (body.rotation * q);
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
			// luego habra que suavizarla (Additive animation?)
			anim.transform.Rotate (Vector3.up, 180);
			// Girar camara
			cam.SetTrigger ("Turn");
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
		if ( InputX.GetKeyDown ( PlayerActions.Jump ) && !OnAir && !cannotJump )
		{
			/// Trigger animaciones de salto
			anim.SetTrigger ("Jump");
		}
	}
	#endregion

	#region CALLBACKS
	private void FixedUpdate () 
	{
		/// Cada cliente conrtola SOLO su personaje
//		if (!hasAuthority) return;
// Esta linea esta comentada
// para trabajar con el
// personaje sin red!

		if (cannotWork) return;

		var dir = InputX.GetMovement ();
		Movement (dir);
		Rotation (dir);
	}

	private void Update() 
	{
		/// Cada cliente conrtola SOLO su personaje
//		if ( !isClient || !hasAuthority) return;
// Esta linea esta comentada
// para trabajar con el
// personaje sin red!
		UpdateCapsule ();
		if (cannotWork) return;
		JumpCheck ();
		PUCheck ();
	}

	private void Awake() 
	{
		/// Referencias internas
		playerCapsule = GetComponent<CapsuleCollider> ();
		body = GetComponent<Rigidbody> ();
		body.centerOfMass = Vector3.zero;
		SpeedMul = runSpeedMul;
	} 

	private void OnCollisionEnter( Collision col ) 
	{
		/// Checks de colision
		switch (col.gameObject.tag)
		{
			case "Floor":
				if (OnAir)
				{
					OnAir = false;
					anim.SetTrigger ( "Land" );
				}
			break;
		}
	}
	#endregion

	#region POWER UP
	void PUCheck ()
	{
		if (cannotWork || cannotJump || powerUp==PU.NONE) return;
		if (InputX.GetKeyDown (PlayerActions.PowerUp))
		{
			StartCoroutine ("PU_" + powerUp.ToString ());
			PowerUp.ShowPU (-1, true);
			powerUp = PU.NONE;
		}
	}

	IEnumerator PU_SpeedUp () 
	{
		float mul = 2f;

		SpeedMul *= mul;
		charSpeed *= mul;
		yield return new WaitForSeconds (2f);
		SpeedMul /= mul;
		charSpeed /= mul;
	}
	IEnumerator PU_Shield () 
	{
		var obj = Instantiate (shield);
		obj.transform.SetParent (anim.transform);
		obj.transform.localPosition = Vector3.up * 0.358f;
		shielded=true;

		var endTime = Time.time + 3f;
		while (Time.time<endTime && shielded) yield return null;

		Destroy (obj);
		shielded=false;
	}
//	IEnumerator PU_Bomb () 
//	{
//
//	}
	#endregion

	#region STUFF
	public IEnumerator BlockPlayer ( float t, bool onAir=false ) 
	{
		this.OnAir = OnAir;
		cannotWork = true;
		Moving = false;
		yield return null;
		anim.ResetTrigger ("Hit");
		yield return new WaitForSeconds (t);
		cannotWork = false;
		anim.ResetTrigger ("Hit");
	}

	public IEnumerator Tentaculo ( Transform hook ) 
	{
		playerCapsule.enabled = false;
		var ogPos = anim.transform.localPosition;
		var ogRot = anim.transform.localRotation;
		anim.transform.Translate (0.35f * -currentDirection, 0f, 0f);
		anim.applyRootMotion = true;
		anim.transform.SetParent (hook);
		yield return new WaitForSeconds (0.45f);
		cam.SetTrigger ("GoDown");
		yield return new WaitForSeconds (0.6f);

		OnAir=true;
		var animPos = Vector3.zero;
		animPos.y = anim.transform.position.y;
		transform.position = animPos;

		anim.transform.SetParent (transform);
		anim.applyRootMotion = false;
		anim.transform.localPosition = ogPos;
		anim.transform.localRotation = ogRot;
		yield return new WaitForSeconds (0.1f);
		playerCapsule.enabled = true;
	}

	void UpdateCapsule () 
	{
		if (anim.transform.hasChanged)
		{
			var newCenter = playerCapsule.center;
			newCenter.z = anim.transform.localPosition.z;
			playerCapsule.center = newCenter;
		}
	}
	#endregion
}
