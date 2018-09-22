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
		internal Hero owner;
		internal Rigidbody body;
		internal CapsuleCollider capsule;

		// ——— Air-Ground check ——— 
		private bool touchingFloorLastFrame;
		private float leaveFloorTime;
		private const float OnAirThreshold = 0.5f;
		private RaycastHit[] hits = new RaycastHit[4];
		#endregion

		private void Update () 
		{
			#region FALL CHECK
			if (TouchingFloor ()) 
			{
				// If hit floor from air (+ in mid-air animation), land character
				if (owner.OnAir && owner.anim.IsInState ("Locomotion.Air.Mid_Air"))
				{
					owner.anim.SetTrigger ("Land");
					owner.OnAir = false;
				}
				// Reset fall-check
				touchingFloorLastFrame = true;
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

		private void Start () 
		{
			body = GetComponent<Rigidbody> ();
			body.centerOfMass = Vector3.zero;
			capsule = GetComponent<CapsuleCollider> ();
		}

		private bool TouchingFloor () 
		{
			var pos = owner.transform.position + (Vector3.up*0.1f);
			int n = Physics.RaycastNonAlloc (pos, -Vector3.up, hits, .2f, 1<<8);

			// Check if any hit was actual floor (not self)
			bool touching = false;
			for (int i=0; i!=n; i++)
			{
				if (hits[i].collider.tag != "Player")
					touching = true;
			}
			return touching;
		}
	} 
}
