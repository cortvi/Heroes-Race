using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/* The Hero mesh & animator, as well as this script,
 * are contained on a single GameObject.
 * 
 * But the rigidbody and colliders that control its physic movement (angular rotation)
 * and such are separated, this is to ensure that networked movement is fluid. */

public partial class Character 
{
	#region DATA
	public const float Speed = 15.0f;

	// External references
	internal Rigidbody driver;
	private CapsuleCollider capsule;
	#endregion

	#region LOCOMOTION
	[ClientCallback] private void Motion () 
	{
		if (!hasAuthority) return;
		// Get input from local Client Player to Server
		Cmd_Motion (-Input.GetAxis ("Horizontal"));
	}

	private void Move () 
	{
		// Stick with the Driver
		transform.position = ComputeDriverPosition ();
		transform.rotation = driver.rotation; 
		#warning rotation?? not really
	}
	#endregion

	#region CALLBACKS
	private void Update () 
	{
		Motion ();
		Move ();
	}

	// Be sure authority is set
	protected void Start () 
	{
		if (isClient)
		{
			// Client-side Player Drivers are kinematic
			// and locomotion is networked and then interpolated
			driver.isKinematic = true;

			// All collision checks and such
			// are checked ONLY Server-side
			capsule.enabled = false;
		}
		else
		if (isServer)
		{
			// Applying physics to an interpolated rigidbody is bad
			driver.interpolation = RigidbodyInterpolation.None;
		}
	}

	private void Awake () 
	{
		// Spawn Driver (both Client & Server)
		var prefab = Resources.Load<GameObject> ("Prefabs/Character_Driver");
		driver = Instantiate (prefab).GetComponent<Rigidbody> ();
		driver.name = identity + "_Driver";
		driver.centerOfMass = Vector3.zero;
		capsule = driver.GetComponent<CapsuleCollider> ();
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
public partial class Character : NetBehaviour
{
	#region DATA
	[SyncVar] internal Game.Heroes identity;
	#endregion

	#region UTILS
	[Command (channel = Channels.DefaultUnreliable)]
	private void Cmd_Motion (float input)
	{
		// Apply physics on Server
		var velocity = input * Speed * Time.deltaTime;
		driver.angularVelocity = velocity * Vector3.up;

		// Propagate motion to ALL Clients
		var data = new DriverData ()
		{
			position = driver.position,
			rotation = driver.rotation
		};
		Rpc_PropagateMotion (data);
	}

	[ClientRpc (channel = Channels.DefaultUnreliable)]
	private void Rpc_PropagateMotion (DriverData data) 
	{
		#warning should check variation of movement maybe?
		driver.MovePosition (data.position);
		driver.MoveRotation (data.rotation);
	} 
	#endregion

	#region HELPERS
	// This struct encapsulates all data 
	// to be propagated about the character
	[Serializable] public struct DriverData 
	{
		public Vector3 position;
		public Quaternion rotation;
	}
	#endregion
}
