using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HeroesRace 
{
	public class Driver : MonoBehaviour 
	{
		#region DATA
		// ——— Helpers ———
		internal Rigidbody body;
		internal CapsuleCollider capsule;
		internal Hero owner;

		// ——— Air-Ground check ——— 
		private int groundCollisions;
		private bool touchingFloorLastFrame;

		private float leaveFloorTime;
		private const float OnAirThreshold = 0.3f;
		private const float MinFloorHeight = 0.2f;
		#endregion

		private void Update () 
		{
			#region FALL CHECK
			if (groundCollisions != 0)
			{
				// Reset fall-check
				touchingFloorLastFrame = true;

				// If hit floor from air (+ in mid-air animation), land character
				if (owner.OnAir && owner.anim.IsInState ("Locomotion.Air.Mid_Air"))
				{
					owner.anim.SetTrigger ("Land");
					owner.OnAir = false;
				}
			}
			else
			// Don't start time if on-air already
			if (!owner.OnAir)
			{
				// Start timer
				if (touchingFloorLastFrame) 
				{
					leaveFloorTime = Time.time + OnAirThreshold;
					touchingFloorLastFrame = false;
				}
				else
				// Must stay on-air some time before starting falling
				if (Time.time > leaveFloorTime) owner.OnAir = true;
			} 
			#endregion
		}

		private void OnCollisionEnter (Collision collision) 
		{
			if (collision.collider.tag == "Ground")
			{
				// Find lowest contact point and check if it's low enough
				bool isFloor = collision.contacts.Min (c => c.point.y) <= MinFloorHeight;
				if (isFloor) groundCollisions += 1;
			}
		}

		private void OnCollisionExit (Collision collision) 
		{
			if (collision.collider.tag == "Ground") 
			{
				// Find lowest contact point and check if it's low enough
				bool isFloor = collision.contacts.Min (c => c.point.y) <= MinFloorHeight;
				if (isFloor) groundCollisions -= 1;

				// Esto no deberia pasar nunca, pero bueno...
				if (groundCollisions < 0) 
				{
					print ("Algo va mal! (was colliding with: " + collision.collider.name);
					groundCollisions = 0;
				}
			}
		}

		private void Awake () 
		{
			capsule = GetComponent<CapsuleCollider> ();
			body = GetComponent<Rigidbody> ();
			body.centerOfMass = Vector3.zero;
		}
	} 
}
