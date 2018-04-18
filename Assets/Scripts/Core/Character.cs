using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/* The Hero mesh & animator, as well as this script,
 * are contained on a single GameObject.
 * 
 * But the rigidbody and colliders that control its physic movement (angular rotation)
 * are separated, this is to ensure that networked movement is fluid. */

public partial class Character : NetBehaviour
{
	#region DATA
	public const float Speed = 15.0f;

	/// External references
	private Rigidbody driver;
	private CapsuleCollider capsule;
	#endregion

	#region LOCOMOTION
	[ClientCallback] private void Motion () 
	{
		/// Only recieve input
		/// ONLY from local Player
		if (!isLocal) return;

		/// Get A-D input
		var input = Vector3.up;
		input *= -Input.GetAxis("Horizontal");

		/// Move Character with physics
		driver.angularVelocity = (input * Speed) * Time.deltaTime;

		/// Send to server
		Cmd_PropagateMotion (driver.rotation);
	}

	private void Move () 
	{
		/// Stick with the Driver
		transform.position = ComputeDriverPosition ();
		transform.rotation = driver.rotation;
	}
	#endregion

	#region CALLBACKS
	private void Update () 
	{
		Motion ();
		Move ();
	}

	protected override void Start () 
	{
		base.Start ();

		/// Spawn driver
		var prefab = Resources.Load<GameObject> ("Prefabs/Character_Driver");
		driver = Instantiate (prefab).GetComponent<Rigidbody> ();

		/// Set up
		driver.name = identity + "_Driver";
		driver.centerOfMass = Vector3.zero;
		capsule = driver.GetComponent<CapsuleCollider> ();

		/// Non-local Player Drivers are kinematic
		/// and locomotion is networked and
		/// then interpolated for the rest
		if (!isLocal) 
		{
			capsule.enabled = false;
			driver.isKinematic = true;
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

/// Network-related behaviour
public partial class Character : NetBehaviour
{
	#region DATA
	[SyncVar] public Heroes identity;

	[SyncVar (hook = "OnChangedDriverState")]
	private Quaternion driverState;
	#endregion

	/// Informs all Players that this Character is moving to given rotation
	[Command] private void Cmd_PropagateMotion (Quaternion motion) 
	{
		driverState = motion;
	}

	/// Changes target motion for this non-local Character
	private void OnChangedDriverState (Quaternion newState) 
	{
		Debug.LogError ("is this happening?");

		/// Only non-local players move this way
		if (isLocal) return;
		driver.MoveRotation (newState);

		Debug.LogError (driver.isKinematic);
		Debug.LogError (newState);
	}
}
