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

public class Character : NetworkBehaviour
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
		/// Get A-D input
		var input = Vector3.up;
		input *= Input.GetAxis ("Horizontal");

		/// Assign input to the player speed
		movingSpeed = input * -speed;
	}

	private void Rotation () 
	{

	}

	private void Move () 
	{
		/// Apply moving speed to the driver
		driver.angularVelocity = movingSpeed * Time.deltaTime;

		/// Stick with it
		transform.position = ComputePosition ();
		transform.rotation = driver.rotation;
	}
	#endregion

	#region CALLBACKS
	[ClientCallback] private void Update () 
	{
		if (!hasAuthority) return;

		Movement ();
		Rotation ();
		Move ();
	}

	/// This is called ONLY on the local client owner of each player
	[ClientCallback] public override void OnStartAuthority () 
	{
		base.OnStartAuthority ();

		/// Spawn driver
		var prefab = Resources.Load<GameObject> ("Prefabs/Character_Driver");
		var driver = Instantiate (prefab);
		driver.name = identity + "_Driver";

		/// Set up references
		capsule = driver.GetComponent<CapsuleCollider> ();
		this.driver = driver.GetComponent<Rigidbody> ();
		this.driver.centerOfMass = Vector3.zero;
	}
	#endregion

	#region HELPERS
	/// Returns the WS position of this Hero,
	/// based on its Driver
	public Vector3 ComputePosition ()
	{
		var pos = capsule.center; pos.y = 0f;           /// Get capsule position, discard height
		return driver.transform.TransformPoint (pos);   /// Return the position in world-space						
	}

	/// Spawns a hero & authorizes its owner
	[Server] public static void Spawn ( Heroes heroToSpawn, NetworkConnection owner ) 
	{
		/// Instantiate Hero object
		var prefab = Resources.Load<Character> ("Prefabs/Heroes/" + heroToSpawn.ToString ());
		var hero = Instantiate (prefab);

		/// Change parameters
		hero.name = "["+owner.connectionId+"] " + heroToSpawn;
		hero.identity = heroToSpawn;

		/// Network spawn
		NetworkServer.SpawnWithClientAuthority (hero.gameObject, owner);
	}
	#endregion
}
