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

public partial class Character : NetworkBehaviour
{
	#region DATA
	public Heroes identity;
	public const float speed = 15.0f;

	/// External references
	private Rigidbody driver;
	private CapsuleCollider capsule;

	/// Internal data
	internal Vector3 movingSpeed;
	#endregion

	#region LOCOMOTION
	private void Movement () 
	{
		if (!hasAuthority) return;

		/// Get A-D input
		var input = Vector3.up;
		input *= Input.GetAxis ("Horizontal");

		/// Assign input to the player speed
		movingSpeed = -(speed * input);
	}

	private void Rotation () 
	{
		
	}

	private void Move () 
	{
		/// Apply motion over the Network
		NetworkMove ();

		/// Stick with the Driver
		transform.position = ComputePosition ();
		transform.rotation = driver.rotation;
	}
	#endregion

	#region CALLBACKS
	[ClientCallback] private void Update () 
	{
		Movement ();
		Rotation ();
		Move ();
	}

	/// Start is called AFTER authority is set
	[ClientCallback] private void Start () 
	{
		/// Spawn driver
		var prefab = Resources.Load<GameObject> ("Prefabs/Character_Driver");
		driver = Instantiate (prefab).GetComponent<Rigidbody> ();

		/// Set up
		driver.name = identity + "_Driver";
		driver.centerOfMass = Vector3.zero;
		capsule = driver.GetComponent<CapsuleCollider> ();

		/// For other player objects, drivers are kinematic
		/// and locomotion is networked and then interpolated
		if (!hasAuthority)
		{
			driver.name.Insert (0, "[OTHER] ");
			driver.isKinematic = true;
		}
	}
	#endregion

	#region HELPERS
	/// Returns the final WS Driver position
	private Vector3 ComputePosition ()
	{
		var pos = capsule.center; pos.y = 0f;           /// Get capsule position, discard height
		return driver.transform.TransformPoint (pos);   /// Return the position in world-space						
	}
	#endregion
}


/// Network-adapted behaviour
public partial class Character : NetworkBehaviour
{
	#region DATA
	[SyncVar] private Quaternion driverState;

	/// This way Driver motion is quickly updated
	public override int GetNetworkChannel () 
	{
		return Channels.DefaultUnreliable;
	}
	#endregion

	/// Moves the player along the Network
	private void NetworkMove () 
	{
		/// Move locally with precision, and
		/// propagates motion over the Network
		if (hasAuthority)
		{
			driver.angularVelocity = movingSpeed * Time.deltaTime;
			Cmd_PropagateMotion (driver.rotation);
		}

		/// Update networked Driver and let rigidbody interpolate it
		else driver.MoveRotation (driverState);
	}

	[Command(channel=Channels.DefaultUnreliable)]
	private void Cmd_PropagateMotion (Quaternion driverState) 
	{
		this.driverState = driverState;
	}
}
