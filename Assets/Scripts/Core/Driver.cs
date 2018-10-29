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

		// ——— Floor check ——— 
		private bool touchingFloorLastFrame;
		private float leaveFloorTime;
		private const float OnAirThreshold = 0.5f;
		private readonly RaycastHit[] hits = new RaycastHit[3];
		#endregion

		#region CALLBACKS
		private void Update () 
		{
			if (TouchingFloor ())
			{
				// If hit floor from air (+ in mid-air animation), land character
				if (owner.OnAir && owner.anim.IsInState ("Locomotion.Air.Mid_Air"))
				{
					owner.OnAir = false;
					SwitchFriction (true);
				}
				// Reset fall-timer
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
				if (Time.time > leaveFloorTime)
				{
					owner.OnAir = true;
					SwitchFriction (false);
				}
			}
		}

		private void Start () 
		{
			body = GetComponent<Rigidbody> ();
			body.centerOfMass = Vector3.zero;
			capsule = GetComponent<CapsuleCollider> ();
		}
		#endregion

		#region HELPERS
		private bool TouchingFloor () 
		{
			var pos = owner.transform.position + (Vector3.up * 0.1f);
			int n = Physics.RaycastNonAlloc (pos, -Vector3.up, hits, .2f, 1 << 8);

			// Check if any hit was actual floor (not self)
			bool touching = false;
			for (int i=0; i!=n; i++)
			{
				if (hits[i].collider != capsule)
					touching = true;
			}
			return touching;
		}

		public void SwitchFriction (bool touchingFloor) 
		{
			float friction = touchingFloor? 0.8f : 0f;
			// On air, remove any friction to allow jumping &
			// falling next to surfaces, but on floor add some friction
			// to avoid infinite slipping
			var material = capsule.material;
			material.staticFriction = friction;
			material.dynamicFriction = friction;
		}
		#endregion
	} 
}
