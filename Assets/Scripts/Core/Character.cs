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

	/// External references
	private Rigidbody driver;
	private CapsuleCollider capsule;
	#endregion

	#region LOCOMOTION
	/// Get input from local Client Player
	[ClientCallback] private void Motion () 
	{
		if (!hasAuthority) return;

		/// Send A-D input to server
		Cmd_Motion (-Input.GetAxis ("Horizontal"));
	}

	/// Stick with the Driver
	private void Move () 
	{
		transform.position = ComputeDriverPosition ();
		transform.rotation = driver.rotation; // ?? not really
	}
	#endregion

	#region CALLBACKS
	private void Update () 
	{
		Motion ();
		Move ();
	}

	/// Be sure authority is set
	protected void Start () 
	{
		/// Spawn driver
		var prefab = Resources.Load<GameObject> ("Prefabs/Character_Driver");
		driver = Instantiate (prefab).GetComponent<Rigidbody> ();

		/// Set up
		driver.name = identity + "_Driver";
		driver.centerOfMass = Vector3.zero;
		capsule = driver.GetComponent<CapsuleCollider> ();

		if (!isServer) 
		{
			/// Client-side Player Drivers are kinematic
			/// and locomotion is networked and then interpolated
			driver.isKinematic = true;

			/// All collision checks and such
			/// are checked ONLY Server-side
			capsule.enabled = false;
		}
	}
	#endregion

	#region HELPERS
	/// Returns the final WS Driver position
	private Vector3 ComputeDriverPosition () 
	{
		var pos = capsule.center; pos.y = 0f;           /// Get capsule position, discard height
		return driver.transform.TransformPoint (pos);   /// Return the position in world-space						
	}
	#endregion
}

/* Everything is processed this way:
 * - Input is get locally from each client
 * - This input is send to the server
 * - In the server take place all the actual physics
 * - All motion and interactions are propagated across the network */ 

/// Network-related behaviour
public partial class Character : NetBehaviour
{
	#region DATA
	[SyncVar] public Heroes identity;

	// net delta time....?
	#endregion

	/// Informs all Players that this Character is moving to given rotation
	[Command (channel = Channels.DefaultUnreliable)]
	private void Cmd_Motion (float input) 
	{
		/// Apply physics
		var velocity = input * Speed * Time.deltaTime;
		driver.angularVelocity = velocity * Vector3.up;

		/// Propagate motion to ALL Clients
		var motion = new DriverMotion () 
		{
			position = driver.position,
			rotation = driver.rotation
		};
		Rpc_PropagateMotion (motion);
	}
	[ClientRpc (channel = Channels.DefaultUnreliable)]
	private void Rpc_PropagateMotion (DriverMotion motion) 
	{
		driver.MovePosition (motion.position);
		driver.MoveRotation (motion.rotation);
	}

	#region HELPERS
	/// This struct encapsulates all data 
	/// to be propagated about the character
	[Serializable] public struct DriverMotion 
	{
		public Vector3 position;
		public Quaternion rotation;
	}
	#endregion
}
