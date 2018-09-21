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
		private bool touchingFloorLastFrame;
		private float leaveFloorTime;
		private const float OnAirThreshold = 0.5f;
		#endregion

		private void Update () 
		{
			#region FALL CHECK
			var b = owner.groundBox;
			// Use a disabled BoxCollider to check if touching ground
			bool touchingFloor = Physics.CheckBox (b.center, b.size/2f, owner.transform.rotation, 1<<8);
			if (touchingFloor) 
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

		private void Awake () 
		{
			capsule = GetComponent<CapsuleCollider> ();
			body = GetComponent<Rigidbody> ();
			body.centerOfMass = Vector3.zero;
		}
	} 
}
