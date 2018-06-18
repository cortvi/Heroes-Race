using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/* The Hero mesh & animator, as well as this script,
 * are contained on a single GameObject.
 * 
 * But the rigidbody and colliders that control its physic movement (angular rotation)
 * and such are separated and only present in the Server.
 * 
 * Clients get their 3D data from the Server and is lerped for a fluid movement. */

public partial class Character 
{
	#region DATA
	internal float Speed = 10.0f;
	internal Rigidbody driver;
	internal SmartAnimator anim;

	private CapsuleCollider capsule;
	#endregion

	#region LOCOMOTION
	[ClientCallback] private void GetMotion () 
	{
		if (!hasAuthority) return;
		float input = -Input.GetAxis ("Horizontal");

		// Update animator
		anim.SetBool ("Moving", input != 0f);

		// Send from Client -> Server
		Cmd_Motion (input);
	}

	private void Rotation () 
	{
		// Get signed facing direction
//		var faceDir = driver.transform.right * (movDir>0? 1f : -1f);
//		var q = Quaternion.LookRotation (faceDir);
//
//		// Lerp for smooth turns
//		transform.rotation = Quaternion.Slerp (transform.rotation, q, Time.deltaTime * 10f);
	}

	private void Move () 
	{
		if (isClient)
		{
			var lerp = Vector3.Lerp (transform.position, syncMotion.position, Time.deltaTime * 7f);
			transform.position = lerp;
		}
		else
		if (isServer) 
		{
			// Stick with the Driver
			transform.position = ComputeDriverPosition ();
		}
	}
	#endregion

	#region SKILLS

	#endregion

	#region CALLBACKS
	private void Update () 
	{
		GetMotion ();
		Rotation ();
		Move ();
	}

	// Be sure authority is set
	protected void Start () 
	{
		if (isClient)
		{
			// Initialize camera
			var cam = Camera.main.gameObject.AddComponent<ClientCamera> ();
			cam.target = this;
		}
		else
		if (isServer)
		{
			// Set up Driver (only present on Server)
			var prefab = Resources.Load<GameObject> ("Prefabs/Character_Driver");
			driver = Instantiate (prefab).GetComponent<Rigidbody> ();
			driver.name = identity + "_Driver";
			driver.centerOfMass = Vector3.zero;

			capsule = driver.GetComponent<CapsuleCollider> ();
		}
	}

	private void Awake () 
	{
		// Get references
		anim = GetComponent<Animator> ().GoSmart ();
	}
	#endregion

	#region HELPERS
	private Vector3 ComputeDriverPosition () 
	{
		var pos = capsule.center; pos.y = 0f;           // Get capsule position, discard height
		return driver.transform.TransformPoint (pos);   // Return the position in world-space						
	}
	#endregion
}

/* Everything is processed this way:
 * - Input is get locally from each client
 * - This input is send to the server
 * - In the server take place all the actual physics
 * - All motion and interactions are propagated across the network */ 

// Network-related behaviour
[NetworkSettings (sendInterval = 0f)]
public partial class Character : NetBehaviour
{
	#region DATA
	[SyncVar] internal Game.Heroes identity;
	[SyncVar] private MotionData syncMotion;
	#endregion

	#region LOCOMOTION
	[Command (channel=1)]
	private void Cmd_Motion (float input) 
	{
		// Apply physics on Server
		var velocity = input * Speed * Time.deltaTime;
		driver.angularVelocity = velocity * Vector3.up;

		// Propagate motion to ALL Clients
		var motion = new MotionData () 
		{
			position = ComputeDriverPosition (),
		};
		syncMotion = motion;
	}
	#endregion

	#region HELPERS
	// Encapsulates all motion data to be propagated
	[Serializable] public struct MotionData  
	{
		public Vector3 position;
	}
	#endregion
}
