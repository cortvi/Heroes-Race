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

public partial class Character 
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

	internal float syncMovingDir;
	internal float input;

	private CapsuleCollider capsule;
	#endregion

	#region LOCOMOTION
	private void CheckInput () 
	{
		if (!hasAuthority) return;

		// Save input for physics
		input = -Input.GetAxis ("Horizontal");
		// Don't update facing direction if not moving!
		if (input != 0) syncMovingDir = input;

		// Update animator
		anim.SetBool ("Moving", input != 0f);
	}

	private void SyncMotion () 
	{
		// Positionate character based on Driver
		transform.position = ComputePosition ();
		transform.rotation = ComputeRotation ();
	}
	#endregion

	#region SKILLS
	private void CheckJump () 
	{
		if (Input.GetKeyDown (KeyCode.Space)) 
		{
			driver.AddForce (Vector3.up * 4f, ForceMode.VelocityChange);
		}
	}
	#endregion

	#region CALLBACKS
	[ClientCallback]
	private void Update () 
	{
		if (!hasAuthority) return;

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
			capsule = driver.GetComponent<CapsuleCollider> ();
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
		var faceDir = driver.transform.right * (syncMovingDir>0? 1f : -1f);
		var q = Quaternion.LookRotation (faceDir);

		// Lerp rotation for smooth turns
		return Quaternion.Slerp (transform.rotation, q, Time.deltaTime * 10f);
	}
	#endregion
}

// Network-related behaviour
[NetworkSettings (channel = 1, sendInterval = 0f)]
public partial class Character : NetBehaviour
{

}
