using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
	#region INTERNAL DATA
	[Header ("References")]
	private Animator anim;				// El Animator del personaje
	private Animator cam;				// El Animator de la camara

	[Header ("PU Refs")]
	public GameObject shield;
//	public GameObject bomb;
	
	[Header ("Params")]
	public float jumpForce;				// Fuerza del salto
	public float charSpeed;				// La velocidad del personaje
	public float runSpeedMul;			// Añadido a la velocidad base de la animacion de correr

	[HideInInspector] public PU powerUp;
	[HideInInspector] public bool shielded;

	[HideInInspector] public Game owner;
	[HideInInspector] public Rigidbody body;	// El 'Rigidbody' que se encarga de algunas físicas del personaje
	[HideInInspector] public bool cannotWork;	// Esta bloqueada la accion del jugador?
	[HideInInspector] public bool cannotJump;   // Esta bloqueado el salto?
	CapsuleCollider playerCapsule;
	#endregion

	#region ANIMATON PARAMS
	public float SpeedMul 
	{
		get { return anim.GetFloat ("SpeedMul"); } 
		set
		{
			anim.SetFloat ("SpeedMul", value);
			Cmd_SetFloat ("SpeedMul", value);
		}
	}
	public bool Moving 
	{
		get { return anim.GetBool ("Moving"); }
		set
		{
			anim.SetBool ("Moving", value);
			Cmd_SetBool ("Moving", value);
		}
	}
	public bool OnAir 
	{
		get { return anim.GetBool ("OnAir"); }
		set
		{
			anim.SetBool ("OnAir", value);
			Cmd_SetBool ("OnAir", value);
		}
	}

	[Command] void Cmd_SetFloat ( string name, float value)  { anim.SetFloat (name, value); }
	[Command] void Cmd_SetBool ( string name, bool value )  { anim.SetBool (name, value); }

	public void SetTrigger ( string name ) 
	{
		anim.SetTrigger (name);
		Cmd_SetTrigger (name);
	}
	[Command] void Cmd_SetTrigger ( string name ) { anim.SetTrigger (name); }

	public void ResetTrigger ( string name ) 
	{
		Cmd_ResetTrigger (name);
	}
	[Command] void Cmd_ResetTrigger ( string name ) { anim.ResetTrigger (name); }

	#region CAMERA ANIMATOR
	public void TriggerCam( string trigger )
	{
		anim.SetTrigger (trigger);
		Cmd_TriggerCam (trigger);
	}

	[Command] void Cmd_TriggerCam( string trigger ) 
	{
		cam.SetTrigger (trigger);
	}
	#endregion
	#endregion

	#region MOVEMENT
	void Movement (float dir) 
	{
		if (dir != 0)
		{
			Moving = true;
			var speed = Vector3.up * charSpeed * -dir * Time.fixedDeltaTime;
			body.angularVelocity = speed;
		}
		else
		{
			body.angularVelocity = Vector3.zero;
			Moving = false;
		}

		Cmd_Movement (body.angularVelocity, body.velocity);
	}
	[Command]
	void Cmd_Movement (Vector3 speed, Vector3 vel) 
	{
		body.angularVelocity = speed;
		body.velocity = vel;
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
			TriggerCam ("Turn");
			currentDirection = dir;
		}
	}
	/// aG9sYSBwaWUgOjM=
	#endregion

	#region JUMPING
	void JumpCheck () 
	{
		if (!InputX.GetKeyDown (PlayerActions.Jump) || OnAir || cannotJump) return;
		anim.SetTrigger ("Jump");
	}
	#endregion

	#region CALLBACKS
	private void FixedUpdate () 
	{
		/// Cada cliente conrtola SOLO su personaje
		if (!hasAuthority) return;
		if (cannotWork) return;

		var dir = InputX.GetMovement ();
		Movement (dir);
		Rotation (dir);
	}

	private void Update() 
	{
		UpdateCapsule ();
		if (isServer || !hasAuthority) return;
		if (cannotWork) return;

		JumpCheck ();
		PUCheck ();
	}

	private void OnCollisionEnter( Collision col ) 
	{
		/// Checks de colision
		if ((col.gameObject.tag == "Floor" || col.gameObject.tag == "Player") && OnAir)
		{
			OnAir = false;
			anim.SetTrigger ("Land");
		}
	}

	private void Awake () 
	{
		/// Referencias internas
		cam = transform.GetChild (1).GetComponent<Animator> ();
		anim = transform.GetChild (0).GetComponent<Animator> ();
		playerCapsule = GetComponent<CapsuleCollider> ();
		body = GetComponent<Rigidbody> ();
		body.centerOfMass = Vector3.zero;
		SpeedMul = runSpeedMul;
	}

	public override void OnStartAuthority () 
	{
		base.OnStartAuthority ();
		if (!isClient) return;
		cam.gameObject.SetActive (true);
	}
	public override void OnStartServer () 
	{
		base.OnStartServer ();
		if (!isServer) return;
		cam.gameObject.SetActive (true);
		var c = cam.GetComponent<Cam> ();
		cam.GetComponent<Camera> ().targetTexture = c.targets[owner.pj];
		c.enabled = false;
	}
	#endregion

	#region POWER UP
	void PUCheck ()
	{
		if (cannotWork || cannotJump || powerUp==PU.NONE) return;
		if (!InputX.GetKeyDown (PlayerActions.PowerUp)) return;

		StartCoroutine ("PU_" + powerUp.ToString ());
		PowerUp.ShowPU (-1, true);
		powerUp = PU.NONE;
	}

	IEnumerator PU_SpeedUp () 
	{
		float mul = 2f;

		SpeedMul *= mul;
		charSpeed *= mul;
		yield return new WaitForSeconds (1.6f);
		SpeedMul /= mul;
		charSpeed /= mul;
	}
//	IEnumerator PU_Shield () 
//	{
//		var obj = Instantiate (shield);
//		obj.transform.SetParent (anim.transform);
//		obj.transform.localPosition = Vector3.up * 0.358f;
//		shielded=true;
//
//		var endTime = Time.time + 3f;
//		while (Time.time<endTime && shielded) yield return null;
//
//		Destroy (obj);
//		shielded=false;
//	}
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
		ResetTrigger ("Hit");
		yield return new WaitForSeconds (t);
		cannotWork = false;
		ResetTrigger ("Hit");
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
		TriggerCam ("GoDown");
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
