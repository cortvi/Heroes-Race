using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	/* The Hero mesh & animator, as well as this script,
	* are contained on a single GameObject.
	* 
	* But the rigidbody and colliders that control its physic movement (angular rotation)
	* and such are separated and only on each Client locally.
	* 
	* Other Clients get their 3D data from the Server and it's lerped for a fluid movement.
	* Server don't get to use physics at all. */

	public sealed class Hero : NetBehaviour 
	{
		#region DATA
		public override string SharedName 
		{
			get { return identity.ToString (); }
		}

		public Heroes identity;
		internal float Speed = 10.0f;

		internal CCStack cc;
		internal SmartAnimator anim;
		internal Rigidbody driver;
		private CapsuleCollider capsule;

		// Locomotion
		internal float input;
		internal float movingDir;

		// Air-Ground check
		private BoxCollider box;
		private float leaveFloorTime;
		private bool touchingFloorLastFrame = true;
		private const float GoOnAirThreshold = 0.3f;
		#endregion

		#region LOCOMOTION
		private void CheckInput () 
		{
			if (!hasAuthority) return;

			// Save input for physics
			input = -Input.GetAxis ("Horizontal");
			// Don't update facing direction if not moving!
			if (input != 0) movingDir = input;

			// Update animator
			anim.SetBool ("Moving", input != 0f);
		}

		private void SyncMotion () 
		{
			// Positionate character based on Driver
			transform.position = ComputePosition ();
			transform.rotation = ComputeRotation ();
		}

		private void CheckJump () 
		{
			if (anim.GetBool ("OnAir")) return;
			if (!Input.GetKeyDown (KeyCode.Space)) return;

			anim.SetTrigger ("Jump");
			anim.SetBool ("OnAir", true);
		}

		private void CheckFloor () 
		{
			// Check against objects where the player can stand
			bool check = Physics.CheckBox (transform.position, box.size / 2f, transform.rotation, 1<<8);
			if (check)
			{
				// Reset fall-check
				touchingFloorLastFrame = true;

				// If hit floor from air (and in mid-air animation), land character
				if (anim.GetBool ("OnAir") && anim.IsInState ("Locomotion.Air.Mid_Air")) 
				{
					anim.SetTrigger ("Land");
					anim.SetBool ("OnAir", false);
				}
			}
			else
			// If already on-air, don't start timer
			if (!anim.GetBool ("OnAir")) 
			{
				// Start timer
				if (touchingFloorLastFrame)
				{
					leaveFloorTime = Time.time + GoOnAirThreshold;
					touchingFloorLastFrame = false;
				}
				else
				// Must stay on-air some time before starting falling
				if (Time.time > leaveFloorTime) anim.SetBool ("OnAir", true);
			}
		}
		#endregion

		#region ANIMATION EVENTS
		[ClientCallback]
		private void Jump () 
		{
			driver.AddForce (Vector3.up * 6f, ForceMode.VelocityChange);
		}
		#endregion

		#region CALLBACKS
		[ClientCallback]
		private void Update () 
		{
			if (!hasAuthority) return;
			cc.Update ();

			CheckFloor ();
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
			if (isClient)
			{
				if (hasAuthority) 
				{
					// Initialize camera to focus local Client
					var cam = Camera.main.gameObject.AddComponent<ClientCamera> ();
					cam.target = this;

					// Set up Driver (only present on local Client)
					var prefab = Resources.Load<Rigidbody> ("Prefabs/Character_Driver");
					driver = Instantiate (prefab);
					driver.name = identity + "_Driver";
					driver.centerOfMass = Vector3.zero;

					// Get references
					cc = new CCStack (this);
					anim = new SmartAnimator (GetComponent<Animator> (), networked: true);
					capsule = driver.GetComponent<CapsuleCollider> ();
					box = GetComponentInChildren<BoxCollider> ();
				}
				// On not-ownwer Clients:
				// Enable this simple collider to allow interactions between players 
				else GetComponent<CapsuleCollider> ().enabled = true;
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
			var faceDir = driver.transform.right * (movingDir > 0 ? 1f : -1f);
			var q = Quaternion.LookRotation (faceDir);

			// Lerp rotation for smooth turns
			return Quaternion.Slerp (transform.rotation, q, Time.deltaTime * 10f);
		}
		#endregion
	}

	public enum Heroes 
	{
		NONE = -1,

		Espectador,
		Indiana,
		Harley,
		Harry,

		Count
	}
}
