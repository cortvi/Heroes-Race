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

	private Rigidbody driver;
	private CapsuleCollider capsule;
	#endregion

	#region LOCOMOTION

	#endregion

	#region CALLBACKS
	[ClientCallback]
	private void Update () 
	{
		if (!hasAuthority) return;
		/// Stick with its driver
		transform.position = ComputePosition ();
		transform.rotation = driver.rotation;
	}

	[ClientCallback]
	public override void OnStartAuthority () 
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
	/// Spawns a hero & authorizes its owner
	[Server] public static void Spawn ( Heroes heroToSpawn, NetworkConnection owner ) 
	{
		/// Instantiate
		var prefab = Resources.Load<Character> ("Prefabs/Heroes/" + heroToSpawn.ToString ());
		var hero = Instantiate (prefab);

		/// Change parameters
		hero.name = "["+owner.connectionId+"] " + heroToSpawn;
		hero.identity = heroToSpawn;

		/// Network spawn
		NetworkServer.SpawnWithClientAuthority (hero.gameObject, owner);
	}

	/// Returns the WS position of this Hero,
	/// based on its Driver
	public Vector3 ComputePosition () 
	{
		var pos = capsule.center; pos.y = 0f;			/// Get capsule position, discard height
		return driver.transform.TransformPoint (pos);	/// Return the position in world-space						
	}
	#endregion
}
