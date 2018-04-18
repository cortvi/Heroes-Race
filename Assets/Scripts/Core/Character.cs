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

/* Movement works this way:
* - Local Player gets input
* - Input is passed to server
* - Server-side Driver is moved by physics
* - Server-side Driver motion is propagated
* - Client-side Players follow propagated motion with a kinematic rigidbody. */

public partial class Character : NetworkBehaviour
{
	#region DATA
	public const float speed = 15.0f;

	/// External references
	private Rigidbody driver;
	private CapsuleCollider capsule;
	#endregion

	#region LOCOMOTION
	private void Locomotion () 
	{
		if (Networker.DedicatedClient) 
		{

		}
//		Movement ();

	}

	private void Movement () 
	{
		if (!isLocal) return;

		/// Get A-D input
		var input = Vector3.up;
		input *= Input.GetAxis ("Horizontal");

		/// Assign input to the player speed
		//movingSpeed = -(speed * input);
	}

	private void Rotation () 
	{
		
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
		Move ();
	}

	/// Start is called AFTER authority is set, both on client and server
	private void Start () 
	{
		/// Spawn driver
		var prefab = Resources.Load<GameObject> ("Prefabs/Character_Driver");
		driver = Instantiate (prefab).GetComponent<Rigidbody> ();

		/// Set up
		driver.name = identity + "_Driver";
		driver.centerOfMass = Vector3.zero;
		capsule = driver.GetComponent<CapsuleCollider> ();

		/// Client-side Player Drivers are kinematic
		/// and locomotion is networked and then interpolated
		if (!isServer)
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
public partial class Character : NetworkBehaviour
{
	#region DATA
	[SyncVar] public Heroes identity;
	[SyncVar] public Vector3 Velocity;
	[SyncVar] private Quaternion driverState;

	public bool isLocal;
	/// When spawned on the net, each Player sets
	/// his own hero as local, so they can control it
	[TargetRpc] public void Target_SetLocal (NetworkConnection target) 
	{
		isLocal = true;
	}
	#endregion

	#region LOCOMOTION

	#endregion
}
