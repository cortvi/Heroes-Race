using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/* The Hero mesh & animator, as well as this script,
* are contained on a single GameObject.
* 
* The rigidbody and colliders that control its physic movement (angular rotation)
* and such are separated in a Driver GameObject and only present on the Server.
* 
* All the Clients get their 3D data from the Server, then it's lerped for a fluid movement.
* No Client makes actual physics logic, everything is computed on the Server and passed to the Clients. */
namespace HeroesRace 
{
	public sealed partial class /* COMMON */ Hero : NetBehaviour 
	{
		internal float movingDir; // This is used by the Hero Camera

		private void Update () 
		{
			if (isServer)
			{
				blocks.Read ();
				SyncMotion ();
			}
			// On Clients, follow given motion
			else KeepMotion ();
		}
	}

	public sealed partial class /* SERVER */ Hero 
	{
		#region DATA
		// ——— Helpers ———
		internal Driver driver;
		internal CCStack blocks;
		internal BoxCollider groundBox;

		// ——— Animation ———
		public SmartAnimator anim;
		public bool Moving 
		{
			get { return anim.GetBool ("Moving"); }
			set { anim.SetBool ("Moving", value); }
		}
		public bool OnAir 
		{
			get { return anim.GetBool ("OnAir"); }
			set { anim.SetBool ("OnAir", value); }
		}

		// ——— Locomotion ———
		internal float speed = 10.0f;
		internal float input;
		#endregion

		#region LOCOMOTION
		public void Movement (float axis) 
		{
			if (!blocks[CCs.Moving])
			{
				// Save input for later physics
				if (axis != 0f) movingDir = axis;
				input = axis;
			}
			else input = 0f;

			if (input != 0f) Moving = true;
			else Moving = false;
		}
		public void Jumping () 
		{
			if (!OnAir
			&& !blocks[CCs.Jumping]) 
			{
				anim.SetTrigger ("Jump");
				OnAir = true;
			}
		}
		
		private void SyncMotion () 
		{
			Vector3 pos;
			Quaternion rot;
			float angular;

			// Positionate character based on Driver & propagate over Net
			transform.position = pos = ComputePosition ();
			transform.rotation = rot = ComputeRotation ();

			// Send angular speed to allow client-side prediction
			if (input != 0f)
				angular = (Mathf.Rad2Deg * driver.body.angularVelocity.y);
			else
				angular = 0f;

			// Send all data to Client
			Rpc_SyncMotion (pos, rot, angular, movingDir);
		}
		#endregion

		#region PHYISICS
		[ServerCallback]
		private void Jump () 
		{
			driver.body.AddForce
				(Vector3.up * 6f, ForceMode.VelocityChange);
		}
		#endregion

		#region CALLBACKS
		[ServerCallback]
		private void FixedUpdate () 
		{
			var velocity = Vector3.up * (input * speed) * Time.fixedDeltaTime;
			// Don't modify speed if CCed, because probably a external force is moving the Hero
			if (!blocks[CCs.Moving]) driver.body.angularVelocity = velocity;
		}

		protected override void OnServerAwake () 
		{
			// Get references
			anim = new SmartAnimator (GetComponent<Animator> (), networked: true);
			groundBox = GetComponentInChildren<BoxCollider> ();
			blocks = new CCStack (this);

			#warning adding a Hero camera for testing in the server!
			OnStartOwnership ();
		}
		#endregion

		#region HELPERS
		private Vector3 ComputePosition () 
		{
			// Get capsule position, discard height
			var pos = driver.capsule.center; pos.y = 0f;
			// Return the position in world-space
			return driver.transform.TransformPoint (pos);
		}

		private Quaternion ComputeRotation () 
		{
			// Get signed facing rotation
			var faceDir = driver.transform.right * (movingDir > 0 ? 1f : -1f);
			var q = Quaternion.LookRotation (faceDir);

			// Lerp rotation for smooth turns
			return Quaternion.Slerp (transform.rotation, q, Time.deltaTime * 10f);
		}
		#endregion

		#region CC STACK
		public class CCStack 
		{
			#region DATA + CTOR + IDXER
			private readonly Hero owner;
			private readonly Dictionary<string, CCs> collection;
			private CCs summary;

			public CCStack (Hero owner) 
			{
				this.owner = owner;
				collection = new Dictionary<string, CCs> ();
			}

			public bool this[CCs cc] 
			{
				get { return summary.HasFlag (cc); }
			}
			#endregion

			#region UTILS
			public void Read () 
			{
				summary = CCs.None;
				foreach (var e in collection)
					summary = summary.SetFlag (e.Value);
			}

			public void Add (string name, CCs cc, float duration = -1f, bool unique = true) 
			{
				// Add timestamp to remove uniqueness
				if (!unique) name += Time.time;

				collection.Add (name, cc);
				if (duration > 0f) owner.StartCoroutine 
						(RemoveAfter (name, duration));
			}

			public void Remove (string name) 
			{
				collection.Remove (name);
			}

			private IEnumerator RemoveAfter (string name, float duration) 
			{
				float release = Time.time + duration;
				while (Time.time < release) yield return null;
				collection.Remove (name);
			}
			#endregion
		}
		#endregion
	}

	public sealed partial class /* CLIENT */ Hero 
	{
		#region DATA
		internal Vector3 netPosition;     // Exact real position
		internal Quaternion netRotation;  // Transform rotation
		internal float netAngular;        // Speed around tower

		internal HeroCamera cam; 
		#endregion

		[ClientRpc (channel = 2)]
		private void Rpc_SyncMotion (Vector3 pos, Quaternion rot, float angular, float movingDir) 
		{
			netPosition = pos;
			netRotation = rot;
			netAngular = angular;
		}

		private void KeepMotion () 
		{
			// Height is lerped always
			var pos = transform.position;
			pos.y = Mathf.Lerp (pos.y, netPosition.y, Time.deltaTime * 30f);
			transform.position = pos;

			// Lerp to real position if stopped, otherwise move around tower in given speed
			if (netAngular == 0f) transform.position = Vector3.Lerp (transform.position, netPosition, Time.deltaTime * 10f);
			else transform.RotateAround (Vector3.zero, Vector3.up, netAngular * Time.deltaTime);

			// Rotation is always lerped too
			transform.rotation = Quaternion.Slerp (transform.rotation, netRotation, Time.deltaTime * 20f);
		}

		internal override void OnStartOwnership () 
		{
			if (!cam) 
			{
				// Initialize camera to focus local Client
				cam = Camera.main.gameObject.AddComponent<HeroCamera> ();
				cam.target = this;
			}
		}
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

	[Flags] public enum CCs 
	{
		Moving = 1 << 0,
		Rotating = 1 << 1,
		Jumping = 1 << 2,

		// ——— Specials ———
		Locomotion = Moving | Rotating | Jumping,
		None = 0
	}
}
