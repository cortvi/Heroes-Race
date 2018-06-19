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
	public override string SharedName 
	{
		get { return identity.ToString (); }
	}

	internal float Speed = 10.0f;
	internal Rigidbody driver;
	internal SmartAnimator anim;

	private CapsuleCollider capsule;
	#endregion

	#region LOCOMOTION
	private void SendInput () 
	{
		if (!hasAuthority) return;
		float input = -Input.GetAxis ("Horizontal");

		// Update animator
		anim.SetBool ("Moving", input != 0f);

		// Send from Client -> Server
		Cmd_ProcessInput (input);
	}

	private void SyncMotion () 
	{
		// Interpolated from values sent by Server
		var pos = Vector3.Lerp (transform.position, syncPosition, positionInterpolation);
		var rot = Quaternion.Slerp (transform.rotation, syncRotation, rotationInterpolation);
		transform.position = pos;
		transform.rotation = rot;
	}
	#endregion

	#region SKILLS
	private void CheckJump () 
	{
		if (Input.GetKeyDown (KeyCode.Space))
		{
			Cmd_Jump ();
		}
	}
	#endregion

	#region CALLBACKS
	private void Update () 
	{
		if (isClient) 
		{
			SendInput ();
			SyncMotion ();

			CheckJump ();
		}
		else
		if (isServer) 
		{
			// Propagate motion to ALL Clients
			transform.position = syncPosition = ComputePosition ();
			transform.rotation = syncRotation = ComputeRotation ();
		}
	}

	private void FixedUpdate () 
	{
		if (isServer) 
		{
			// Apply physics
			var velocity = input * Speed * Time.deltaTime;
			driver.angularVelocity = velocity * Vector3.up;
		}
	}

	protected override void OnStart () // When authority is set 
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

	protected override void OnAwake () 
	{
		// Get references
		anim = GetComponent<Animator> ().GoSmart ();
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
		var faceDir = driver.transform.right * (syncMovingDir>0? 1f : -1f);
		var q = Quaternion.LookRotation (faceDir);

		// Lerp rotation for smooth turns
		return Quaternion.Slerp (transform.rotation, q, Time.deltaTime * 10f);
	}
	#endregion
}

/* Everything is processed this way:
 * - Input is get locally from each client
 * - This input is send to the server
 * - In the server take place all the actual physics
 * - All motion and interactions are propagated across the network */ 

// Network-related behaviour
[NetworkSettings (channel = 1, sendInterval = 0f)]
public partial class Character : NetBehaviour
{
	#region DATA
	[SyncVar] public Game.Heroes identity;
	[SyncVar] internal float syncMovingDir;

	[SyncVar] private Vector3 syncPosition;
	[SyncVar] private Quaternion syncRotation;

	[Range (0f, 1f)] public float positionInterpolation;
	[Range (0f, 1f)] public float rotationInterpolation;

	private float input;
	#endregion

	#region LOCOMOTION
	[Command (channel=1)]
	private void Cmd_ProcessInput (float input) 
	{
		// Save input for physics
		this.input = input;

		// Don't update facing direction if not moving!
		if (input != 0) syncMovingDir = input;
	}
	#endregion

	#region SKILLS
	[Command]
	private void Cmd_Jump () 
	{
		driver.AddForce (Vector3.up * 4f, ForceMode.VelocityChange);
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
