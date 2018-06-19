using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/* The Hero mesh & animator, as well as this script,
 * are contained on a single GameObject.
 * 
 * But the rigidbody and colliders that control its physic movement (angular rotation)
 * and such are separated and only on each Client locally.
 * 
 * Other clients get their 3D data from the Server and it's lerped for a fluid movement. */

public partial class Character : NetBehaviour
{
	#region DATA
	public override string SharedName 
	{
		get { return identity.ToString (); }
	}

	public Game.Heroes identity;
	internal float Speed = 10.0f;

	internal Rigidbody driver;
	internal SmartAnimator anim;

	// Locomotion
	internal float input;
	internal float movingDir;
	internal bool jumping;

	private CapsuleCollider capsule;

	// Air-Ground check
	private BoxCollider box;
//	private float leftFloorTime;
//	private bool touchingFloorLastFrame;
//	private const float GoOnAirThreshold = 0.3f;
	#endregion

	#region LOCOMOTION
	private void CheckInput () 
	{
		if (!hasAuthority) return;

		// Save input for physics
		input = -Input.GetAxis ("Horizontal");
		// Don't update facing direction if not moving!
		if (input != 0) movingDir = input;

		// Update animator
		anim.SetBool ("Moving", input != 0f);
	}

	private void SyncMotion () 
	{
		// Positionate character based on Driver
		transform.position = ComputePosition ();
		transform.rotation = ComputeRotation ();
	}

	private void CheckJump () 
	{
		if (jumping) return;
		if (anim.GetBool ("OnAir")) return;
		if (!Input.GetKeyDown (KeyCode.Space)) return;

		anim.SetTrigger ("Jump");
		jumping = true;
	}

	private void CheckFloor () 
	{
		bool check = Physics.CheckBox (transform.position, box.size / 2f, transform.rotation, 1<<8);
		if (check && jumping) anim.SetTrigger ("Land");
		anim.SetBool ("OnAir", check);
	}
	#endregion

	#region ANIMATION EVENTS
	[ClientCallback] private void Jump () 
	{
		driver.AddForce (Vector3.up * 6f, ForceMode.VelocityChange);
		anim.SetBool ("OnAir", true);
	}
	#endregion

	#region CALLBACKS
	[ClientCallback]
	private void Update () 
	{
		if (!hasAuthority) return;
		CheckFloor ();
		SyncMotion ();
		CheckInput ();
		CheckJump ();
	}

	[ClientCallback]
	private void FixedUpdate () 
	{
		if (!hasAuthority) return;

		// Apply physics
		var velocity = input * Speed * Time.deltaTime;
		driver.angularVelocity = velocity * Vector3.up;
	}

	protected override void OnStart () 
	{
		if (isClient && hasAuthority) 
		{
			// Initialize camera
			var cam = Camera.main.gameObject.AddComponent<ClientCamera> ();
			cam.target = this;

			// Set up Driver (only present on local Client)
			var prefab = Resources.Load<GameObject> ("Prefabs/Character_Driver");
			driver = Instantiate (prefab).GetComponent<Rigidbody> ();
			driver.name = identity + "_Driver";
			driver.centerOfMass = Vector3.zero;

			// Get references
			anim = GetComponent<Animator> ().GoSmart ();
			capsule = GetComponent<CapsuleCollider> ();
			box = GetComponent<BoxCollider> ();
		}
		else 
		{
			// On not-ownwer Clients & Server
			// Enable this simple collider to allow interactions
			// between players & with the environment
			GetComponent<CapsuleCollider> ().enabled = true;
		}
	}
	#endregion

	#region HELPERS
	private Vector3 ComputePosition () 
	{
		// Get capsule position, discard height
		var pos = capsule.center;
		pos.y = 0f;

		// Return the position in world-space
		return driver.transform.TransformPoint (pos);						
	}

	private Quaternion ComputeRotation () 
	{
		// Get signed facing direction
		var faceDir = driver.transform.right * (movingDir>0? 1f : -1f);
		var q = Quaternion.LookRotation (faceDir);

		// Lerp rotation for smooth turns
		return Quaternion.Slerp (transform.rotation, q, Time.deltaTime * 10f);
	}
	#endregion
}
